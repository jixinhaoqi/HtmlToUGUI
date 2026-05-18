using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// CSS 解析器：负责解析 HTML 中的 style 标签和行内样式，计算每个节点的最终样式。
    /// </summary>
    public class CssParser
    {
        private static readonly Regex s_CssCommentRegex = new Regex(@"/\*[\s\S]*?\*/", RegexOptions.Compiled);
        private static readonly Regex s_RootFontSizeRegex = new Regex(@"html\s*\{[^}]*font-size\s*:\s*([^;}]+)", RegexOptions.Compiled);
        private static readonly Regex s_CssBlockRegex = new Regex(@"([^{]+)\s*\{([^}]*)\}", RegexOptions.Compiled);
        private static readonly Regex s_PseudoClassRegex = new Regex(@"(?<!\{[^{}]*)(?<!\[[^\[\]]*):([\w-]+(?:\([^)]*\))?)", RegexOptions.Compiled);
        private static readonly Regex s_BracketContentRegex = new Regex(@"\[[^\]]*\]", RegexOptions.Compiled);
        private static readonly Regex s_TokenSplitRegex = new Regex(@"(\s*>\s*|\s+)", RegexOptions.Compiled);
        private static readonly Regex s_TagNameRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_-]*", RegexOptions.Compiled);
        private static readonly Regex s_IdClassNameRegex = new Regex(@"[a-zA-Z_][a-zA-Z0-9_-]*", RegexOptions.Compiled);
        private static readonly Regex s_AttrSelectorRegex = new Regex(@"^([a-zA-Z_][a-zA-Z0-9_-]*)\s*([~|^$*]?=)\s*(?:""([^""]*)""|'([^']*)'|([^""'\s]+))$", RegexOptions.Compiled);
        private static readonly Regex s_SimpleAttrNameRegex = new Regex(@"^([a-zA-Z_][a-zA-Z0-9_-]*)$", RegexOptions.Compiled);
        private static readonly Regex s_CssVarRegex = new Regex(@"var\((--[^),]+)(?:,\s*([^)]+))?\)", RegexOptions.Compiled);
        private static readonly Regex s_EmUnitRegex = new Regex(@"(-?\d+(?:\.\d+)?)em", RegexOptions.Compiled);
        private static readonly Regex s_CssATRegex = new Regex(@"@[^{]*\{(?>[^{}]+|(?<open>\{)|(?<-open>\}))*(?(open)(?!))\}", RegexOptions.Compiled);

        private List<CssRule> _allRules;
        private Dictionary<string, string> _varStyles = new Dictionary<string, string>();
        private int _rootFontSize = 16;
        private List<(int inlines, int ids, int classes, int tags, int order, Dictionary<string, string> props)> _matchedRulesBuffer = new List<(int inlines, int ids, int classes, int tags, int order, Dictionary<string, string> props)>();
        private List<string> _varKeysBuffer = new List<string>();

        /// <summary>
        /// 当前元素匹配的伪类样式，key 是伪类名称，value 是对应的样式属性字典
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> CurrentPseudoClassStyles { get; private set; } =
            new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// HTML 根字体大小
        /// </summary>
        public int RootFontSize => _rootFontSize;

        /// <summary>
        /// 解析 HTML 文档中所有 style 标签内的 CSS 规则
        /// </summary>
        /// <param name="doc">HTML 文档</param>
        public virtual void ParseStyleSheet(HtmlDocument doc)
        {
            _allRules = new List<CssRule>();
            _varStyles = new Dictionary<string, string>();
            var styleNodes = doc.DocumentNode.SelectNodes("//style");
            if (styleNodes == null) return;

            int order = 0;
            foreach (var node in styleNodes)
            {
                string css = node.InnerText;
                css = s_CssCommentRegex.Replace(css, "");
                css = s_CssATRegex.Replace(css, "");
                var rootMatch = s_RootFontSizeRegex.Match(css);
                if (rootMatch.Success)
                {
                    _rootFontSize = (int)UnitParser.Parse(rootMatch.Groups[1].Value.Trim(), 16f);
                }

                var blocks = s_CssBlockRegex.Matches(css);
                foreach (Match block in blocks)
                {
                    string selectorText = block.Groups[1].Value.Trim();
                    string propsText = block.Groups[2].Value.Trim();
                    var chains = ParseSelectorList(selectorText);
                    if (chains == null || chains.Count == 0) continue;

                    var props = InlineStyleParser.Parse(propsText);
                    if (props.Count == 0) continue;

                    foreach (var prop in props)
                    {
                        if (prop.Key.StartsWith("--"))
                            _varStyles[prop.Key] = prop.Value;
                    }

                    _allRules.Add(new CssRule
                    {
                        SelectorText = selectorText,
                        Selectors = chains,
                        Properties = props,
                        Order = order++
                    });
                }
            }
        }

        /// <summary>
        /// 计算指定 HTML 节点的最终样式（继承 + CSS 规则 + 行内样式）
        /// </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="parentStyle">父节点的样式</param>
        /// <returns>最终样式</returns>
        public virtual Dictionary<string, string> ResolveStyles(HtmlNode node, Dictionary<string, string> parentStyle)
        {
            CurrentPseudoClassStyles.Clear();

            var final = new Dictionary<string, string>(parentStyle ?? new Dictionary<string, string>());
            var matchedRules = _matchedRulesBuffer;
            matchedRules.Clear();

            if (_allRules != null)
            {
                foreach (var rule in _allRules)
                {
                    if (MatchesAnySelector(node, rule))
                    {
                        if (HasPseudoClass(rule.SelectorText))
                        {
                            var resolved = ResolveVariablesInProps(new Dictionary<string, string>(rule.Properties));
                            foreach (Match m in s_PseudoClassRegex.Matches(rule.SelectorText))
                            {
                                string pseudoKey = m.Groups[1].Value.Trim();
                                if (CurrentPseudoClassStyles.TryGetValue(pseudoKey, out var existing))
                                {
                                    foreach (var kv in resolved)
                                        existing[kv.Key] = kv.Value;
                                }
                                else
                                {
                                    CurrentPseudoClassStyles[pseudoKey] = new Dictionary<string, string>(resolved);
                                }
                            }
                            continue;
                        }

                        int ids = 0, classes = 0, tags = 0;
                        foreach (var chain in rule.Selectors)
                        {
                            if (MatchesSelectorChain(node, chain))
                            {
                                var sp = GetSpecificity(chain);
                                ids = sp.ids; classes = sp.classes; tags = sp.tags;
                                break;
                            }
                        }
                        matchedRules.Add((0, ids, classes, tags, rule.Order,
                            ResolveVariablesInProps(new Dictionary<string, string>(rule.Properties))));
                    }
                }
            }

            matchedRules.Sort((a, b) =>
            {
                int cmp = a.inlines.CompareTo(b.inlines);
                if (cmp != 0) return cmp;
                cmp = a.ids.CompareTo(b.ids);
                if (cmp != 0) return cmp;
                cmp = a.classes.CompareTo(b.classes);
                if (cmp != 0) return cmp;
                cmp = a.tags.CompareTo(b.tags);
                if (cmp != 0) return cmp;
                return a.order.CompareTo(b.order);
            });

            foreach (var r in matchedRules)
            {
                foreach (var kv in r.props)
                    final[kv.Key] = UnitParser.ConvertRelativeUnits(final, kv.Key, kv.Value, _rootFontSize);
            }

            // 行内样式最高优先级
            var inline = InlineStyleParser.Parse(node.GetAttributeValue("style", ""));
            foreach (var kv in inline)
            {
                string val = kv.Value;
                if (val.Contains("var("))
                    val = ResolveVar(val);
                final[kv.Key] = UnitParser.ConvertRelativeUnits(final, kv.Key, val, _rootFontSize);
            }

            ResolveEmAgainstOwnFontSize(final);
            ResolveWebkitBackgroundClip(final);
            return final;
        }

        /// <summary>
        /// 后处理：根据 CSS 规范，非 font-size 属性的 em 值基于元素自身的 font-size 解析
        /// </summary>
        private void ResolveEmAgainstOwnFontSize(Dictionary<string, string> final)
        {
            if (!final.TryGetValue("font-size", out string ownFontSize)) return;
            float fs = UnitParser.Parse(ownFontSize, rootFontSize: _rootFontSize);
            foreach (var key in final.Keys.ToArray())
            {
                if (key == "font-size" || !final[key].Contains("em")) continue;
                final[key] = s_EmUnitRegex.Replace(final[key], match =>
                {
                    float n = float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                    return (n * fs) + "px";
                });
            }
        }

        private void ResolveWebkitBackgroundClip(Dictionary<string, string> final)
        {
            if ((!final.TryGetValue("background-clip", out string bgClip) || bgClip != "text") &&
                (!final.TryGetValue("-webkit-background-clip", out string webkitBgClip) || webkitBgClip != "text")) return;
            if (final.ContainsKey("color")||final.ContainsKey("-webkit-text-fill-color"))
            {
                bool haveColor = final.TryGetValue("-webkit-text-fill-color", out string color) || final.TryGetValue("color", out color);
                bool haveBackgroundColor = ColorParser.TryParseBackgroundColor(final, out Color backgroundColor);
                if (haveBackgroundColor)
                {
                    string colorString ="#"+ ColorUtility.ToHtmlStringRGBA(backgroundColor);
                    final["color"] = haveColor?colorString:"black";
                }
            }
            List<string> colors = new List<string>();
            foreach (var kv in final)
            {
                if (ColorParser.IsBackgroundColor(kv.Key))
                    colors.Add(kv.Key);
            }
            foreach (var k in colors)
            {
                final.Remove(k);
            }
        }
        #region 选择器解析

        protected virtual bool HasPseudoClass(string selectorText)
        {
            string noBrackets = s_BracketContentRegex.Replace(selectorText, "");
            return noBrackets.Contains(':');
        }

        protected virtual List<CompoundSelectorChain> ParseSelectorList(string text)
        {
            var list = new List<CompoundSelectorChain>();
            foreach (string part in SplitByComma(text))
            {
                var chain = ParseCompoundChain(part.Trim());
                if (chain != null) list.Add(chain);
            }
            return list;
        }

        protected virtual List<string> SplitByComma(string text)
        {
            var parts = new List<string>();
            int depth = 0;
            int start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '[') depth++;
                else if (c == ']') depth--;
                else if (c == ',' && depth == 0)
                {
                    parts.Add(text.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }
            if (start < text.Length)
                parts.Add(text.Substring(start).Trim());
            return parts;
        }

        protected virtual CompoundSelectorChain ParseCompoundChain(string text)
        {
            var chain = new CompoundSelectorChain();
            var sequences = new List<SimpleSelectorSequence>();
            var relations = new List<Combinator>();

            var tokens = s_TokenSplitRegex.Split(text).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            if (tokens.Length == 0) return null;

            sequences.Add(ParseSimpleSequence(tokens[0]));
            relations.Add(Combinator.None);

            for (int i = 1; i < tokens.Length; i++)
            {
                string token = tokens[i].Trim();
                if (token == ">")
                {
                    if (i + 1 >= tokens.Length) break;
                    sequences.Add(ParseSimpleSequence(tokens[++i]));
                    relations.Add(Combinator.Child);
                }
                else
                {
                    if (token != ">" && token.Length > 0)
                    {
                        sequences.Add(ParseSimpleSequence(token));
                        relations.Add(Combinator.Descendant);
                    }
                }
            }

            while (relations.Count < sequences.Count) relations.Add(Combinator.Descendant);

            chain.Sequences = sequences;
            chain.Relations = relations.ToArray();
            return chain;
        }

        protected virtual SimpleSelectorSequence ParseSimpleSequence(string text)
        {
            var seq = new SimpleSelectorSequence { Classes = new HashSet<string>(), Attrs = new List<AttrSelector>() };
            int pos = 0;

            if (!text.StartsWith(".") && !text.StartsWith("#") && !text.StartsWith("[") && !text.StartsWith(":"))
            {
                var match = s_TagNameRegex.Match(text);
                if (match.Success)
                {
                    seq.Tag = match.Value.ToLower();
                    pos = match.Length;
                }
            }

            while (pos < text.Length)
            {
                if (text[pos] == '#')
                {
                    var match = s_IdClassNameRegex.Match(text.Substring(pos + 1));
                    if (match.Success) { seq.Id = match.Value; pos += 1 + match.Length; }
                    else break;
                }
                else if (text[pos] == '.')
                {
                    var match = s_IdClassNameRegex.Match(text.Substring(pos + 1));
                    if (match.Success) { seq.Classes.Add(match.Value); pos += 1 + match.Length; }
                    else break;
                }
                else if (text[pos] == '[')
                {
                    int end = text.IndexOf(']', pos);
                    if (end == -1) break;
                    string attrStr = text.Substring(pos + 1, end - pos - 1);
                    var attr = ParseAttrSelector(attrStr);
                    if (attr != null) seq.Attrs.Add(attr);
                    pos = end + 1;
                }
                else break;
            }
            return seq;
        }

        protected virtual AttrSelector ParseAttrSelector(string attrStr)
        {
            if (string.IsNullOrEmpty(attrStr)) return null;
            var attr = new AttrSelector();
            var match = s_AttrSelectorRegex.Match(attrStr);
            if (match.Success)
            {
                attr.Name = match.Groups[1].Value;
                string op = match.Groups[2].Value.TrimEnd('=');
                attr.Operator = op.Length > 0 ? op[0] : '=';
                attr.Value = match.Groups[3].Value + match.Groups[4].Value + match.Groups[5].Value;
            }
            else
            {
                match = s_SimpleAttrNameRegex.Match(attrStr);
                if (match.Success) attr.Name = match.Value;
                else return null;
            }
            return attr;
        }

        #endregion

        #region 选择器匹配

        protected static bool MatchesSelectorChain(HtmlNode node, CompoundSelectorChain chain)
        {
            if (chain.Sequences.Count == 0) return false;
            int seqIndex = chain.Sequences.Count - 1;
            HtmlNode current = node;

            while (seqIndex >= 0)
            {
                if (!MatchesSequence(current, chain.Sequences[seqIndex]))
                    return false;

                if (seqIndex > 0)
                {
                    Combinator relation = chain.Relations[seqIndex];
                    if (relation == Combinator.Child)
                    {
                        current = current.ParentNode;
                        if (current == null) return false;
                    }
                    else
                    {
                        HtmlNode ancestor = current.ParentNode;
                        bool matched = false;
                        while (ancestor != null)
                        {
                            if (MatchesSequence(ancestor, chain.Sequences[seqIndex - 1]))
                            { matched = true; current = ancestor; break; }
                            ancestor = ancestor.ParentNode;
                        }
                        if (!matched) return false;
                    }
                }
                seqIndex--;
            }
            return true;
        }

        protected static bool MatchesSequence(HtmlNode node, SimpleSelectorSequence seq)
        {
            if (!string.IsNullOrEmpty(seq.Tag) && !node.Name.Equals(seq.Tag, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!string.IsNullOrEmpty(seq.Id) && node.Id != seq.Id)
                return false;
            if (seq.Classes.Count > 0)
            {
                var nodeClasses = node.GetClasses()?.ToList() ?? new List<string>();
                if (!seq.Classes.IsSubsetOf(nodeClasses)) return false;
            }
            foreach (var attr in seq.Attrs)
            {
                string nodeVal = node.GetAttributeValue(attr.Name, null);
                if (nodeVal == null) return false;
                if (!string.IsNullOrEmpty(attr.Value))
                {
                    switch (attr.Operator)
                    {
                        case '=': if (nodeVal != attr.Value) return false; break;
                        case '^': if (!nodeVal.StartsWith(attr.Value)) return false; break;
                        case '$': if (!nodeVal.EndsWith(attr.Value)) return false; break;
                        case '*': if (!nodeVal.Contains(attr.Value)) return false; break;
                        case '|': if (nodeVal != attr.Value && !nodeVal.StartsWith(attr.Value + "-")) return false; break;
                        case '~': if (!nodeVal.Split(' ').Contains(attr.Value)) return false; break;
                        default: return false;
                    }
                }
            }
            return true;
        }

        protected static bool MatchesAnySelector(HtmlNode node, CssRule rule)
        {
            foreach (var chain in rule.Selectors)
            {
                if (MatchesSelectorChain(node, chain))
                    return true;
            }
            return false;
        }

        protected static (int ids, int classes, int tags) GetSpecificity(CompoundSelectorChain chain)
        {
            int ids = 0, classes = 0, tags = 0;
            foreach (var seq in chain.Sequences)
            {
                if (!string.IsNullOrEmpty(seq.Id)) ids++;
                if (!string.IsNullOrEmpty(seq.Tag)) tags++;
                classes += seq.Classes.Count;
                classes += seq.Attrs.Count;
            }
            return (ids, classes, tags);
        }

        #endregion

        #region CSS 变量

        protected virtual string ResolveVar(string value, int depth = 10)
        {
            if (depth <= 0) return value;
            return s_CssVarRegex.Replace(value, m =>
            {
                string varName = m.Groups[1].Value.Trim();
                string fallback = m.Groups[2].Value.Trim();
                if (_varStyles.TryGetValue(varName, out string resolved))
                    return ResolveVar(resolved, depth - 1);
                return string.IsNullOrEmpty(fallback) ? m.Value : ResolveVar(fallback, depth - 1);
            });
        }

        protected virtual Dictionary<string, string> ResolveVariablesInProps(Dictionary<string, string> props)
        {
            var keys = _varKeysBuffer;
            keys.Clear();
            keys.AddRange(props.Keys);
            foreach (var key in keys)
            {
                string val = props[key];
                if (val.Contains("var("))
                    props[key] = ResolveVar(val);
            }
            return props;
        }

        #endregion
    }
}

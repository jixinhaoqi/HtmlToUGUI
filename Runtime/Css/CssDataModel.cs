using System.Collections.Generic;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// CSS 样式规则
    /// </summary>
    public class CssRule
    {
        public string SelectorText;
        public List<CompoundSelectorChain> Selectors;
        public Dictionary<string, string> Properties;
        public int Order;
    }

    /// <summary>
    /// 一个复合选择器链（如 "div .cls > p"）
    /// </summary>
    public class CompoundSelectorChain
    {
        public List<SimpleSelectorSequence> Sequences;
        public Combinator[] Relations;
    }

    /// <summary>
    /// 单个简单选择器序列（如 "div.class#id"）
    /// </summary>
    public class SimpleSelectorSequence
    {
        public string Tag;
        public string Id;
        public HashSet<string> Classes;
        public List<AttrSelector> Attrs;
    }

    /// <summary>
    /// 属性选择器
    /// </summary>
    public class AttrSelector
    {
        public string Name;
        public string Value;
        public char? Operator;
    }

    /// <summary>
    /// 选择器组合符
    /// </summary>
    public enum Combinator
    {
        None,
        Descendant,
        Child
    }
}

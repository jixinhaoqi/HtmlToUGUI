以下出现的符号：...表示任意值，[]表示可选，/表示或
以下的"标签"指的是body以及body以内的标签
## 1. 结构与基准分辨率 (极度重要)
*   **唯一根标签**：必须存在一个最外层的根标签，声明 `id="root"`（或具体的窗口名，如 `loginWindow`）。
*   **1920x1080 基准**：根标签必须在 `style` 中明确指定 `position: absolute; width: 1920px; height: 1080px;`。最大绝对不可超过此尺寸。

## 2. 核心属性规范
所有标签（，**必须**包含：
*   `id="nodeName"`：唯一标识，**必须使用小驼峰命名法（camelCase）**，如 `loginBtn`、`titleTxt`。
	
## 3. style规范（严格遵守）
*   每个标签都要指定实际的长和高，如 `width: 1920px; height: 1080px;`或者`width: 100%; height: 100%;`，这是为了在unity中计算sizeDelta。
*   每个标签都要指定自身相对于父标签的对齐方式，如`style="position: absolute;left: 50%;top: 50%;[transform: translate(-50%, -50%);]"`，所有position的值必须是absolute，这是为了在unity中计算anchoredPosition、pivot。
*   禁止添加的键：`display、margin、margin-...、border、border-...`以及任何影响子标签布局的键。
*   除了滚动列表和输入框和下拉菜单外禁止添加的键：`padding、padding-...`。
*   除了滚动列表外禁止添加的键：`overflow`。
	
## 4. 检查和纠正
*   检查所有标签style，绝对不能使用禁止的样式，检查style中left、top、width、height的值是否准确，如果出现标签溢出或者视觉不正确则重新计算并修改对应的样式值

按照上述规则生成一个登录页面

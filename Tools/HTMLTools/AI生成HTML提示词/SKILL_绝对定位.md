## 1. 标签规范（包括body）
*   只能使用**绝对定位**方式并设定**宽高**，如`style="position: absolute;left: 50%;top: 50%;width: 50%; height:  50%;transform: translate(-50%, -50%);`，数值单位只能是px或者%，禁止使用right/bottom。
*   必须包含id，id值必须使用**小驼峰命名法（camelCase）**，如 `loginBtn`、`titleTxt`。
*   禁止添加`游离的文本`。
*   禁止添加样式`display/margin/margin-.../padding/padding-..`。
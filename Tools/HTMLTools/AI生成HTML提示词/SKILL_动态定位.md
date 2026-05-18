## 1. 核心属性规范
body以及body以内的所有标签，**必须**包含：
*   `data-u-name="nodeName"`：唯一标识，**必须使用小驼峰命名法（camelCase）**，如 `loginBtn`、`titleTxt`。
*   `data-u-left="...px"`：通过`getBoundingClientRect().left`获取。
*   `data-u-top="...px"`：通过`getBoundingClientRect().top`获取。
*   `data-u-width="...px"`：通过`getBoundingClientRect().width`获取。
*   `data-u-height="...px"`：通过`getBoundingClientRect().height`获取。

## 2. 限制（严格遵守）
禁止添加<link>和<script>标签。

按照上述规则生成一个登录页面
#pragma checksum "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Task.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "15fa4c236b3a8584bc5d03ed3f1ec8a1845e14ad"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Frame_Task), @"mvc.1.0.view", @"/Views/Frame/Task.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\_ViewImports.cshtml"
using ASEWEB;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\_ViewImports.cshtml"
using ASEWEB.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"15fa4c236b3a8584bc5d03ed3f1ec8a1845e14ad", @"/Views/Frame/Task.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"66d5228d74e4487e6e62614565f9d7752ba5a6b5", @"/Views/_ViewImports.cshtml")]
    public class Views_Frame_Task : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<ASEWEB.Models.TaskModel>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("type", new global::Microsoft.AspNetCore.Html.HtmlString("text/javascript"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/js/jquery-3.6.0.js"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("method", "post", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("action", new global::Microsoft.AspNetCore.Html.HtmlString("/Frame/Task"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_4 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("style", new global::Microsoft.AspNetCore.Html.HtmlString("background-color: skyblue;"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        #pragma warning restore 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper;
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper;
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("<!DOCTYPE html>\r\n\r\n<html>\r\n");
#nullable restore
#line 4 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Task.cshtml"
  
    ViewData["Title"] = "Task";
    var data = ViewBag.Task;
    var list = ViewBag.list;
    var bg = "white";
    var P_Name = ViewBag.P_Name;
    var P_Value = ViewBag.P_Value;
    var Update_Message = ViewBag.Update_Message;

#line default
#line hidden
#nullable disable
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "15fa4c236b3a8584bc5d03ed3f1ec8a1845e14ad5913", async() => {
                WriteLiteral("\r\n    <meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\">\r\n    ");
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("script", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "15fa4c236b3a8584bc5d03ed3f1ec8a1845e14ad6253", async() => {
                }
                );
                __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
                await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                if (!__tagHelperExecutionContext.Output.IsContentModified)
                {
                    await __tagHelperExecutionContext.SetOutputContentAsync();
                }
                Write(__tagHelperExecutionContext.Output);
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                WriteLiteral(@"
    <title>任務列表</title>
    <script src=""https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"" ""></script>
    <script src=""https://cdn.staticfile.org/popper.js/1.12.5/umd/popper.min.js""></script>
    <script type=""text/javascript"">
        function Take_CommandID(CMD_ID,Status) {
            if (Status > 1)
                window.alert(""不可修改執行中的任務優先權"");
            else {
                var c = document.getElementById(""TaskPriority"");
                var temp = document.getElementById(""TP_Temp"");
                c.src = temp.value +""?CMD_ID_="" +CMD_ID;
            }
        }

        
    </script>
    <style type=""text/css"">
        #TaskColor {
            width: 1140px;
            height: 50px;
            padding: 0px;
            margin-right: 0px;
            float: left;
            position: absolute;
            font-size: 25px;
        }
        #DIV1 {
            width: 1140px;
            height: 300px;
            line-height: 10px;
            pad");
                WriteLiteral(@"ding: 0px;
            margin-right: 0px;
            float: left;
            position: absolute;
            top: 50px;
        }
        #DIV2 {
            width: 440px;
            height: 400px;
            line-height: 10px;
            padding: 0px;
            margin-right: 0px;
            float: left;
            position: absolute;
            top: 400px;
        }
        #DIV3 {
            width: 400px;
            height: 400px;
            line-height: 10px;
            padding: 0px;
            margin-right: 0px;
            float: left;
            position: absolute;
            top: 400px;
            left:540px;
        }
        table {
            border-collapse: collapse
        }
            table tbody {
                display: block;
                overflow-y: auto;
                -webkit-overflow-scrolling: touch;
            }
                table tbody::-webkit-scrollbar {
                    display: none;
                }
           ");
                WriteLiteral(@" table th {
                table-layout: fixed;
                word-break: break-all;
                height: 30px;
                padding: 0px;
                text-align: center;
                font-size: 14px;
            }
        thead, tbody, tfoot tr {
            display: table;
            width: 100%;
            table-layout: fixed; /* even columns width , fix width of table too*/
        }
        thead tr th {
            position: sticky;
            top: 0;
        }

        tfoot tr td {
            position: sticky;
            bottom: 0;
        }
        table thead {
            background-color: #f4f5f7;
            color: #000000;
            table-layout: fixed;
        }
        table td{
            height:30px;
            text-align:center;
        }
        .content {
            /* 省略... */
            white-space: nowrap;
        }
    </style>

");
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "15fa4c236b3a8584bc5d03ed3f1ec8a1845e14ad11207", async() => {
                WriteLiteral(@"
    <div id=""TaskColor"">
        <table>
            <tbody>
                <tr>
                    <td style=""background-color:aqua"">命令送出等待中</td>
                    <td style=""background-color:gold"">BOX抵達Lifter</td>
                    <td style=""background-color:lime"">任務執行中</td>
                    <td style=""background-color:white"">任務尚未執行</td>
                </tr>
            </tbody>
        </table>
    </div>
    <div id=""DIV1"">
        <iframe scrolling=""no"" frameBorder=""0"" style=""height: 300px; width: 1140px"" src=""/Frame/Task_List"">
        </iframe>
    </div>
    ");
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("form", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "15fa4c236b3a8584bc5d03ed3f1ec8a1845e14ad12085", async() => {
                    WriteLiteral("\r\n        <div id=\"DIV2\">\r\n            <table style=\"background-color:blanchedalmond;\" border=\"1\">\r\n                <tbody>\r\n");
#nullable restore
#line 144 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Task.cshtml"
                     if (P_Name.Count > 0)
                    {
                        for (int i = 0; i < P_Name.Count; i++)
                        {

#line default
#line hidden
#nullable disable
                    WriteLiteral("                            <tr>\r\n                                <td>");
#nullable restore
#line 149 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Task.cshtml"
                               Write(P_Name[i]);

#line default
#line hidden
#nullable disable
                    WriteLiteral("<input type=\"hidden\" name=\"PRIORITY_NAME\"");
                    BeginWriteAttribute("value", " value=\"", 4528, "\"", 4546, 1);
#nullable restore
#line 149 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Task.cshtml"
WriteAttributeValue("", 4536, P_Name[i], 4536, 10, false);

#line default
#line hidden
#nullable disable
                    EndWriteAttribute();
                    WriteLiteral(" /></td>\r\n                                <td><input type=\"text\" style=\"text-align:center\" name=\"PRIORITY_VALUE\"");
                    BeginWriteAttribute("value", " value=\"", 4659, "\"", 4678, 1);
#nullable restore
#line 150 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Task.cshtml"
WriteAttributeValue("", 4667, P_Value[i], 4667, 11, false);

#line default
#line hidden
#nullable disable
                    EndWriteAttribute();
                    WriteLiteral(" /></td>\r\n                            </tr>\r\n");
#nullable restore
#line 152 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Task.cshtml"
                        }
                    }

#line default
#line hidden
#nullable disable
                    WriteLiteral("                    <tr>\r\n                        <td colspan=\"2\" align=\"center\"><input type=\"submit\" value=\"確認\" style=\"width:100%;height:100%\" /></td>\r\n                    </tr>\r\n                </tbody>\r\n            </table>\r\n        </div>\r\n    ");
                }
                );
                __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper);
                __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper);
                __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper.Method = (string)__tagHelperAttribute_2.Value;
                __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_2);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_3);
                await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                if (!__tagHelperExecutionContext.Output.IsContentModified)
                {
                    await __tagHelperExecutionContext.SetOutputContentAsync();
                }
                Write(__tagHelperExecutionContext.Output);
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                WriteLiteral("\r\n");
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_4);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n</html>\r\n");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<ASEWEB.Models.TaskModel> Html { get; private set; }
    }
}
#pragma warning restore 1591

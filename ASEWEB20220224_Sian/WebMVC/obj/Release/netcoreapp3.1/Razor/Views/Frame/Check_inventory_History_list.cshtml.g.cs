#pragma checksum "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "01c668de6bd6c8352c50cfae195224d8a0ec7670"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Frame_Check_inventory_History_list), @"mvc.1.0.view", @"/Views/Frame/Check_inventory_History_list.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"01c668de6bd6c8352c50cfae195224d8a0ec7670", @"/Views/Frame/Check_inventory_History_list.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"66d5228d74e4487e6e62614565f9d7752ba5a6b5", @"/Views/_ViewImports.cshtml")]
    public class Views_Frame_Check_inventory_History_list : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<ASEWEB.Models.Check_inventoryModel>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("type", new global::Microsoft.AspNetCore.Html.HtmlString("text/javascript"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/js/jquery-3.6.0.js"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("method", "get", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("action", new global::Microsoft.AspNetCore.Html.HtmlString("/Frame/Check_inventory_History_List"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
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
            WriteLiteral("<!DOCTYPE html>\r\n\r\n");
#nullable restore
#line 3 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml"
  

    ViewData["Title"] = "Check_history";
    var data = ViewBag.Check_Inventory;
    var History = ViewBag.HistoryList;

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n\r\n<html>\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "01c668de6bd6c8352c50cfae195224d8a0ec76705653", async() => {
                WriteLiteral("\r\n    <meta name=\"viewport\" content=\"width=device-width\" />\r\n    <meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\">\r\n    ");
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("script", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "01c668de6bd6c8352c50cfae195224d8a0ec76706058", async() => {
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
    <script>
        function DIV_Close() {
            parent.DIV_Close();
        }
    </script>
    <style type=""text/css"">
        html, body {
            height: 100%;
            width: 100%;
        }

        table {
            border-spacing: 0;
            width: 100%;
        }
        table tbody {
            display: block;
            overflow-y: auto;
            -webkit-overflow-scrolling: touch;
        }
            table tbody::-webkit-scrollbar {
                display: none;
            }
            thead, tbody tr {
                display: table;
                width: 100%;
                table-layout: fixed; /* even columns width , fix width of table too*/
            }
        table th {
            table-layout: fixed;
            word-break: break-all;
            height: 30px;
            padding: 0px;
            text-align: center;
        }
        table thead {
            background-color: #f4f5f7;
            color: #000000;
   ");
                WriteLiteral(@"     }
        thead tr th {
            position: sticky;
            top: 0;
        }
        tfoot tr td {
            position: sticky;
            bottom: 0;
        }

        tfoot tr {
            display: table;
            width: 100%;
            table-layout: fixed;
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
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "01c668de6bd6c8352c50cfae195224d8a0ec76709328", async() => {
                WriteLiteral("\r\n    ");
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("form", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "01c668de6bd6c8352c50cfae195224d8a0ec76709590", async() => {
                    WriteLiteral("\r\n        <table style=\"border:1px #00007f groove;height: 330px;width: 400px;overflow-y:auto;\" cellpadding=\"0\" border=\'1\'>\r\n            <thead style=\"width:100%\">\r\n                <tr>\r\n");
                    WriteLiteral("                    <th height=\"25px\" width=\"85px\">Carousel ID</th>\r\n                    <th height=\"25px\" width=\"66.5px\">Cell ID</th>\r\n");
                    WriteLiteral(@"                    <th height=""25px"" width=""85.5px"">BOX ID</th>
                    <th height=""25px"" width=""80px"">Check Result</th>
                </tr>
            </thead>
            <tbody style=""background-color:white;"" height=""330px"" width=""400px"">
");
#nullable restore
#line 89 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml"
                 if (History.Count != 0)
                {
                    

#line default
#line hidden
#nullable disable
#nullable restore
#line 91 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml"
                     for (int i = 0; i < History.Count; i = i + 4)
                    {

#line default
#line hidden
#nullable disable
                    WriteLiteral("                        <tr>\r\n");
                    WriteLiteral("                            <td width=\"107px\" align=\"center\">");
#nullable restore
#line 96 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml"
                                                        Write(History[i]);

#line default
#line hidden
#nullable disable
                    WriteLiteral("</td>\r\n                            <td width=\"84px\" align=\"center\">");
#nullable restore
#line 97 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml"
                                                       Write(History[i + 1]);

#line default
#line hidden
#nullable disable
                    WriteLiteral("</td>\r\n                            <td width=\"109px\" align=\"center\">");
#nullable restore
#line 98 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml"
                                                        Write(History[i + 2]);

#line default
#line hidden
#nullable disable
                    WriteLiteral("</td>\r\n                            <td width=\"100px\" align=\"center\">");
#nullable restore
#line 99 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml"
                                                        Write(History[i + 3]);

#line default
#line hidden
#nullable disable
                    WriteLiteral("</td>\r\n                        </tr>\r\n");
#nullable restore
#line 101 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml"
                    }

#line default
#line hidden
#nullable disable
#nullable restore
#line 101 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Check_inventory_History_list.cshtml"
                     
                }

#line default
#line hidden
#nullable disable
                    WriteLiteral("            </tbody>\r\n            <tfoot>\r\n                <tr>\r\n                    <td colspan=\"4\"><input style=\"width:100%\" type=\"button\" value=\"關閉\" onclick=\"DIV_Close()\" /></td>\r\n                </tr>\r\n            </tfoot>\r\n        </table>\r\n    ");
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
                WriteLiteral("\r\n\r\n\r\n\r\n");
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper);
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<ASEWEB.Models.Check_inventoryModel> Html { get; private set; }
    }
}
#pragma warning restore 1591

#pragma checksum "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Test.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "a18a2bacceed2ad7f54db0eb7686b83a703f9d02"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Frame_Test), @"mvc.1.0.view", @"/Views/Frame/Test.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a18a2bacceed2ad7f54db0eb7686b83a703f9d02", @"/Views/Frame/Test.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"66d5228d74e4487e6e62614565f9d7752ba5a6b5", @"/Views/_ViewImports.cshtml")]
    public class Views_Frame_Test : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("style", new global::Microsoft.AspNetCore.Html.HtmlString("height:768px"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("style", new global::Microsoft.AspNetCore.Html.HtmlString("background-color: skyblue;"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
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
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("<!DOCTYPE html>\r\n");
#nullable restore
#line 2 "C:\STRONG\Src\MoveIn_20220221\MoveIn_20220221\STRONG\ASEWEB20220224_Sian\WebMVC\Views\Frame\Test.cshtml"
  
    ViewData["Title"] = "Test";

#line default
#line hidden
#nullable disable
            WriteLiteral("<html>\r\n\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "a18a2bacceed2ad7f54db0eb7686b83a703f9d024560", async() => {
                WriteLiteral(@"
        <meta name=""viewport"" content=""width=device-width"" />
        <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"" />
        <script type=""text/javascript"" src=""https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js""></script>
        <title>測試</title>

        <style type=""text/css"">
            #DIV1 {
                width: 830px;
                height: 200px;
                line-height: 1px;
                padding: 0px;
                margin-right: 0px;
                float: left;
                position: absolute;
                top: 50px;
            }

            #DIV2 {
                width: 830px;
                height: 300px;
                line-height: 1px;
                padding: 0px;
                margin-top: 0px;
                margin-right: 0px;
                float: left;
                position: absolute;
                left: 10px;
                top: 360px;
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
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "a18a2bacceed2ad7f54db0eb7686b83a703f9d026539", async() => {
                WriteLiteral("\r\n    ");
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("form", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "a18a2bacceed2ad7f54db0eb7686b83a703f9d026801", async() => {
                    WriteLiteral(@"
        <input type=""button"" style=""height:30px"" value=Test1>&ensp;&ensp;&ensp;
        <input type=""button"" style=""height:30px"" value=Test2>&ensp;&ensp;&ensp;
        <input type=""button"" style=""height:30px"" value=Test3>&ensp;&ensp;&ensp;
        <div id=""DIV1"">
            <fieldset style=""width: 800px;height: 250px"">
                <legend>Stocker</legend>
                <table style=""width: 800px"" align=""left"" frame=""void"" cellpadding=""4"" cellspacing=""0"">
                    <tbody>
                        <tr>
                            <td><label>HubURL:</label></td>
                            <td colspan=""5"">
                                <input style=""width: 300px"" type=""text"" id=""stock_HubURL""");
                    BeginWriteAttribute("value", " value=\"", 1888, "\"", 1896, 0);
                    EndWriteAttribute();
                    WriteLiteral(">\r\n                            </td>\r\n                            <td><label>HubPWD:</label></td>\r\n                            <td><input style=\"width: 40px\" type=\"text\" id=\"stock_HubPWD\"");
                    BeginWriteAttribute("value", " value=\"", 2084, "\"", 2092, 0);
                    EndWriteAttribute();
                    WriteLiteral(@"></td>
                            <td><input style=""height:30px"" type=""button"" value=""online""></td>
                            <td><input style=""height:30px"" type=""button"" value=""offline""></td>
                        </tr>
                        <tr>
                            <td colspan=""6""><label>車子動作</label></td>
                            <td colspan=""2""><label>刪帳</label></td>
                            <td colspan=""2""><label>盤點</label></td>
                        </tr>
                        <tr>
                            <td><label>Action</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_Action""></td>
                            <td><label>客戶碼</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_CMcode""></td>
                            <td><label>Target</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_Target""></td>
                            <td>");
                    WriteLiteral(@"<label>CarouselID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_del_CarouselID""></td>
                            <td><label>Cmd ID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_check_CmdID""></td>
                        </tr>
                        <tr>
                            <td><label>Car ID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_CarID""></td>
                            <td><label>Position:M</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_PosM""></td>
                            <td><label>T_CELL_ID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_TcellID""></td>
                            <td><label>CellID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_del_CellID""></td>
                            <td><");
                    WriteLiteral(@"label>CarouselID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_check_CarouselID""></td>
                        </tr>
                        <tr>
                            <td><label>Box ID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_BoxID""></td>
                            <td><label>Source</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_Source""></td>
                            <td><label>CmdID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_CmdID""></td>
                            <td colspan=""2"" align=""right""><input style=""height:30px"" type=""button"" value=""C031""></td>
                            <td><label>Action</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_check_Action""></td>
                        </tr>
                        <tr>
                   ");
                    WriteLiteral(@"         <td><label>Batch No</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_BatchNo""></td>
                            <td><label>S_CELL_ID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""stock_S_CELL_ID""></td>
                            <td colspan=""2"" align=""right""><input style=""height:30px"" type=""button"" value=""Send""></td>
                            <td colspan=""2"" align=""right""><input style=""height:30px"" type=""button"" value=""C050""></td>
                        </tr>
                        <tr>
                            <td><label>Soteria</label></td>
                            <td colspan=""5""><input style=""width: 60px"" type=""text"" id=""stock_Soteria""></td>
                            <td colspan=""2"" align=""right""><input style=""height:30px"" type=""button"" value=""C020""></td>
                        </tr>
                        <tr>
                            <td><label>盤點</label></td>
                 ");
                    WriteLiteral(@"           <td><label>CarouselID</label></td>
                            <td colspan=""2""><input style=""width: 150px"" type=""text"" id=""stock_sum_check_CarouselID""></td>
                            <td><label>Action</label></td>
                            <td colspan=""2""><input style=""width: 150px"" type=""text"" id=""stock_sum_check_Action""></td>
                        </tr>
                    </tbody>
                </table>
            </fieldset>
        </div>
        <div id=""DIV2"">
            <fieldset style=""width: 805px;height: 280px"">
                <legend>Shutter Car</legend>
                <table style=""width: 500px"" align=""left"" frame=""void"" cellpadding=""4"" cellspacing=""0"">
                    <tbody>
                        <tr>
                            <td><label>Cmd ID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""Shutter_CmdID""></td>
                            <td></td>
                            <td><input style=""height:30px""");
                    WriteLiteral(@" type=""button"" value=""Online""></td>
                            <td><input style=""height:30px"" type=""button"" value=""Offline""></td>
                        </tr>
                        <tr>
                            <td><label>Action</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""shutter_Action""></td>
                            <td><label>Source</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=shutter_Source></td>
                            <td><label>刪帳</label></td>
                        </tr>
                        <tr>
                            <td><label>Car ID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""shutter_CarID""></td>
                            <td><label>Target</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=shutter_Target></td>
                            <td><label>Car ID</label></td>
                          ");
                    WriteLiteral(@"  <td><input style=""width: 60px"" type=""text"" id=shutter_del_Target></td>
                        </tr>
                        <tr>
                            <td><label>Box ID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""shutter_BoxID""></td>
                            <td></td>
                            <td><input style=""height:30px"" type=""button"" value=""Send""></td>
                            <td><label>Box ID</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""shutter_del_BoxID""</td>
                        </tr>
                        <tr>
                            <td><label>Batch No</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=""shutter_BatchNo""></td>
                        </tr>
                        <tr>
                            <td><label>Position:M</label></td>
                            <td><input style=""width: 60px"" type=""text"" id=shutter_PosM></td>");
                    WriteLiteral(@"
                        </tr>
                        <tr>
                            <td><label>Action</label></td>
                            <td><input style=""width: 200px"" type=""text"" id=shutter_sum_Action></td>
                        </tr>
                        <tr>
                            <td><label>Src/Tar</label></td>
                            <td><input style=""width: 250px"" type=""text"" id=shutter_sum_SrcTar></td>
                        </tr>
                    </tbody>
                </table>
            </fieldset>
        </div>
    ");
                }
                );
                __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper);
                __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
                await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                if (!__tagHelperExecutionContext.Output.IsContentModified)
                {
                    await __tagHelperExecutionContext.SetOutputContentAsync();
                }
                Write(__tagHelperExecutionContext.Output);
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                WriteLiteral("\r\n\r\n\r\n");
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591

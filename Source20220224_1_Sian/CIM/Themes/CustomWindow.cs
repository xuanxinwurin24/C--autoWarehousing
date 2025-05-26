using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CIM
{
    /// <summary>
    /// 按照步驟 1a 或 1b 操作，然後執行步驟 2 以在 XAML 文件中使用此自定義控件。
    ///
    /// 步驟 1a) 在當前項目中存在的 XAML 文件中使用該自定義控件。
    /// 將此 XmlNamespace 特性添加到要使用該特性的標記文件的根
    /// 元素中:
    ///
    /// xmlns:MyNamespace="clr-namespace:WindowDemo"
    ///
    ///
    /// 步驟 1b) 在其他項目中存在的 XAML 文件中使用該自定義控件。
    /// 將此 XmlNamespace 特性添加到要使用該特性的標記文件的根
    /// 元素中:
    ///
    /// xmlns:MyNamespace="clr-namespace:WindowDemo;assembly=WindowDemo"
    ///
    /// 您還需要添加一個從 XAML 文件所在的項目到此項目的項目引用，
    /// 並重新生成以避免編譯錯誤:
    ///
    /// 在解決方案資源管理器中右擊目標項目，然後依次單擊
    /// “添加引用”->“項目”->[瀏覽查找並選擇此項目]
    ///
    ///
    /// 步驟 2)
    /// 繼續操作並在 XAML 文件中使用控件。
    ///
    /// <MyNamespace:CustomWindow/>
    ///
    /// </summary>
    public class CustomWindow : Window
    {
        public CustomWindow()
        {
            DefaultStyleKey = typeof(CustomWindow);
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (SizeToContent == SizeToContent.WidthAndHeight)
                InvalidateMeasure();
        }

        #region Window Commands

        private void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode != ResizeMode.NoResize;
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
            //SystemCommands.CloseWindow(this);
        }

        private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }


        private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element == null)
                return;

            var point = WindowState == WindowState.Maximized ? new Point(0, element.ActualHeight)
                : new Point(Left + BorderThickness.Left, element.ActualHeight + Top + BorderThickness.Top);
            point = element.TransformToAncestor(this).Transform(point);
            SystemCommands.ShowSystemMenu(this, point);
        }

        #endregion
    }
}

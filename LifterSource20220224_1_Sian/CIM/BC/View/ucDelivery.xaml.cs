using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;

namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for ucDelivery.xaml
    /// </summary>
    public partial class ucDelivery : UserControl
    {
        public ucDelivery()
        {
            InitializeComponent();
            btnNormal.Tag = "1";
            btnIssue.Tag = "0";
            btnConfidence.Tag = "0";
            btnProb.Tag = "0";
        }
        public void Initial()
        {
            ucNormal.Initial();
            ucIssue.Initial();
            ucProb.Initial();
            ucConf.Initial();
        }

        private void btnNormal_Click(object sender, RoutedEventArgs e)
        {
            btnNormal.Tag = "1";
            btnIssue.Tag = "0";
            btnConfidence.Tag = "0";
            btnProb.Tag = "0";
            ucNormal.Visibility = Visibility.Visible;
            ucIssue.Visibility = Visibility.Collapsed;
            ucConf.Visibility = Visibility.Collapsed;
            ucProb.Visibility = Visibility.Collapsed;
        }

        private void btnIssue_Click(object sender, RoutedEventArgs e)
        {
            btnNormal.Tag = "0";
            btnIssue.Tag = "1";
            btnConfidence.Tag = "0";
            btnProb.Tag = "0";
            ucNormal.Visibility = Visibility.Collapsed;
            ucIssue.Visibility = Visibility.Visible;
            ucConf.Visibility = Visibility.Collapsed;
            ucProb.Visibility = Visibility.Collapsed;
        }

        private void btnConfidence_Click(object sender, RoutedEventArgs e)
        {
            btnNormal.Tag = "0";
            btnIssue.Tag = "0";
            btnConfidence.Tag = "1";
            btnProb.Tag = "0";
            ucNormal.Visibility = Visibility.Collapsed;
            ucIssue.Visibility = Visibility.Collapsed;
            ucConf.Visibility = Visibility.Visible;
            ucProb.Visibility = Visibility.Collapsed;
        }

        private void btnProb_Click(object sender, RoutedEventArgs e)
        {
            btnNormal.Tag = "0";
            btnIssue.Tag = "0";
            btnConfidence.Tag = "0";
            btnProb.Tag = "1";
            ucNormal.Visibility = Visibility.Collapsed;
            ucIssue.Visibility = Visibility.Collapsed;
            ucConf.Visibility = Visibility.Collapsed;
            ucProb.Visibility = Visibility.Visible;
        }
    }
}

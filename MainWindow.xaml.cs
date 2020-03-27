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

namespace SqlColumnRemover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (rdbAdd.IsChecked.Value)
            {
                this.executeColumnsAdd();
                return;
            }

            if (rdbRemove.IsChecked.Value)
            {
                this.executeColumnsRemove();
                return;
            }
        }

        private void executeColumnsAdd()
        {

        }

        private void executeColumnsRemove()
        {
            TextRange textRange = new TextRange(txtSrc.Document.ContentStart, txtSrc.Document.ContentEnd);
            if (String.IsNullOrEmpty(txtColumns.Text))
            {
                return;
            }

            String[] targetColumns = txtColumns.Text.Replace(" ", "").Split(',');
            String src = textRange.Text;
            String[] lines = src.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            String header = lines[0].Replace("\"", "");
            header = header.Replace("`", "");
            String strCol = header.Substring(header.IndexOf("(") + 1, header.IndexOf(")") - header.IndexOf("(") - 1);
            String[] columns = strCol.Split(new[] { "`,`", "," }, StringSplitOptions.None);
            int targetIndex;
            String[] result = new String[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                if (String.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                if (lines[i].StartsWith("INSERT"))
                {
                    String tmpHead = lines[i];
                    for (int j = 0; j < targetColumns.Length; j++)
                    {
                        result[i] = tmpHead.Replace(",`" + targetColumns[j] + "`", "");
                        result[i] = result[i].Replace(", `" + targetColumns[j] + "`", "");
                        result[i] = result[i].Replace("," + targetColumns[j], "");
                        result[i] = result[i].Replace(", " + targetColumns[j], "");
                        tmpHead = result[i];
                    }

                    continue;
                }

                string tmpRes = lines[i];

                for (int j = 0; j < targetColumns.Length; j++)
                {
                    targetIndex = Array.IndexOf(columns, targetColumns[j]) - j;
                    int startIdx;

                    if (targetIndex > 0)
                    {
                        startIdx = tmpRes.Select((c, v) => new { c, v }).Where(x => x.c == ',').Skip(targetIndex - 1).FirstOrDefault().v;
                    }
                    else
                    {
                        startIdx = tmpRes.Select((c, v) => new { c, v }).Where(x => x.c == ',').FirstOrDefault().v;
                    }
                    var endIdx = tmpRes.Select((c, v) => new { c, v }).Where(x => x.c == ',').Skip(targetIndex).FirstOrDefault().v;
                    tmpRes = tmpRes.Remove(startIdx, endIdx - startIdx);
                }

                if (!String.IsNullOrEmpty(tmpRes) && !String.IsNullOrWhiteSpace(tmpRes))
                {
                    result[i] = tmpRes;
                }

            }

            string res = String.Join("\r\n", result);
            txtRes.Document.Blocks.Clear();
            txtRes.Document.Blocks.Add(new Paragraph(new Run(res)));
        }

        private void txtSrc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtRes != null)
            {
                txtRes.Document.Blocks.Clear();
            }
             
        }

        private void rdbRemove_Checked(object sender, RoutedEventArgs e)
        {
            lblValues.Visibility = Visibility.Visible;
            txtValues.Visibility = Visibility.Visible;
        }

        private void rdbRemove_Unchecked(object sender, RoutedEventArgs e)
        {
            lblValues.Visibility = Visibility.Hidden;
            txtValues.Visibility = Visibility.Hidden;
        }
    }
}

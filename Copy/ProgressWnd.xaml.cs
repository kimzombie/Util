using System.Windows;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;


namespace Copy {
	/// <summary>
	/// ProgressWnd.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ProgressWnd : Window {
		public ProgressWnd() {
			InitializeComponent();

			Left = ( SystemParameters.WorkArea.Width / 2 ) - ( Width / 2 );
			Top = ( SystemParameters.WorkArea.Height / 2 ) - ( Height / 2 );
		}

		public void Show( Process process ) {
			Task task = CmdReader( process );
			ShowDialog();
		}

		private async Task CmdReader( Process process ) {
			string str;
			while ( ( str = await process.StandardOutput.ReadLineAsync() ) != null ) {
				progressText.Text += str;
				progressText.ScrollToEnd();
			}

			process.Close();

			progressText.Text += "\n\n작업 완료!!";
			Hide();
		}
	}
}

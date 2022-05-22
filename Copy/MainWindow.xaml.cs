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
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace Copy {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {
		private CommonOpenFileDialog	fileDialog		= new CommonOpenFileDialog();
		private ProcessStartInfo		cmd				= new ProcessStartInfo();
		private Process					process			= new Process();
		private StringBuilder			sbCommand		= new StringBuilder();
		private List<int>				listNumber		= new List<int>();
		private List<int>				listResultValue	= new List<int>();
		private Random					random          = new Random();


		public MainWindow() {
			InitializeComponent();

			Left = ( SystemParameters.WorkArea.Width / 2 ) - ( Width / 2 );
			Top = ( SystemParameters.WorkArea.Height / 2 ) - ( Height / 2 );

			fileDialog.IsFolderPicker = true;

			cmd.FileName = @"cmd";
			cmd.CreateNoWindow = true;
			cmd.UseShellExecute = false;
			cmd.RedirectStandardInput = true;
			cmd.RedirectStandardOutput = true;
			cmd.RedirectStandardError = true;

			process.StartInfo = cmd;
			process.StartInfo.WorkingDirectory = "C:\\Windows\\System32";
			process.EnableRaisingEvents = false;

			cbRandomFiles.IsChecked = false;
			tbRandomFileNum.IsEnabled = false;
		}

		private void OnBtnSrc( object sender, RoutedEventArgs e ) {
			if( fileDialog.ShowDialog() == CommonFileDialogResult.Ok ) {
				tbSrcPath.Text = fileDialog.FileName;
			}
		}

		private void OnBtnDest( object sender, RoutedEventArgs e ) {
			if ( fileDialog.ShowDialog() == CommonFileDialogResult.Ok ) {
				tbDestPath.Text = fileDialog.FileName;
			}
		}

		private void OnBtnCopy( object sender, RoutedEventArgs e ) {
			process.Start();

			sbCommand.Clear();
			sbCommand.Append( @"robocopy ");
			sbCommand.Append( tbSrcPath.Text );
			sbCommand.Append( " " );
			sbCommand.Append( tbDestPath.Text );
			sbCommand.Append( " " );

			int num = 0;
			if ( cbRandomFiles.IsChecked.Value && int.TryParse( tbRandomFileNum.Text, out num ) ) {
				string[] files = System.IO.Directory.GetFiles( tbSrcPath.Text );

				listNumber.Clear();
				for ( int i = 0; i < files.Length; ++i ) {
					listNumber.Add( i );
				}

				listResultValue.Clear();
				for ( int i = 0; i < num; ++i ) {
					int index = random.Next( listNumber.Count );
					listResultValue.Add( listNumber[index] );

					listNumber.RemoveAt( index );
				}

				for ( int i = 0; i < listResultValue.Count; ++i ) {
					sbCommand.Append( "\"" );
					sbCommand.Append( System.IO.Path.GetFileName( files[listResultValue[i]] ) );
					sbCommand.Append( "\" " );
				}
			}

			sbCommand.Append( "/copy:DAT " );

			if( cbE.IsChecked.Value ) { // 하위 디렉터리 복사
				sbCommand.Append( "/e " );
			}

			if( cbDcopyT.IsChecked.Value ) { // 타임스탬프 유지
				sbCommand.Append( "/dcopy:T " );
			}

			if( cbRetryNum.IsChecked.Value ) { // 실패한 복사에 대한 재시도 횟수 지정
				sbCommand.Append( "/r:" );
				sbCommand.Append( tbRetryNum.Text );
				sbCommand.Append( " " );
			}

			sbCommand.Append( "/mt" );
			sbCommand.Append( Environment.NewLine );

			process.StandardInput.Write( sbCommand.ToString() );
			process.StandardInput.Close();

			process.WaitForExit();
			process.Close();

			MessageBox.Show( "작업 완료", "Robocopy" );
		}

		private void OnCheckRetryNum( object sender, RoutedEventArgs e ) {
			tbRetryNum.IsEnabled = cbRetryNum.IsChecked.Value;
		}

		private void OnUncheckRetryNum( object sender, RoutedEventArgs e ) {
			tbRetryNum.IsEnabled = cbRetryNum.IsChecked.Value;
		}

		private void OnCheckRandomFiles( object sender, RoutedEventArgs e ) {
			tbRandomFileNum.IsEnabled = cbRandomFiles.IsChecked.Value;
		}

		private void OnUncheckRandomFile( object sender, RoutedEventArgs e ) {
			tbRandomFileNum.IsEnabled = cbRandomFiles.IsChecked.Value;
		}
	}
}

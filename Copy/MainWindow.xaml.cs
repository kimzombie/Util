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
using System.IO;


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
				tbFromPath.Text = fileDialog.FileName;
			}
		}

		private void OnBtnDest( object sender, RoutedEventArgs e ) {
			if ( fileDialog.ShowDialog() == CommonFileDialogResult.Ok ) {
				tbToPath.Text = fileDialog.FileName;
			}
		}

		private void OnBtnCopy( object sender, RoutedEventArgs e ) {
			int lastIndex = tbFromPath.Text.LastIndexOf( '\\' );
			string folder = tbFromPath.Text.Substring( lastIndex, tbFromPath.Text.Length - lastIndex );

			sbCommand.Clear();
			sbCommand.Append( @"robocopy ");
			sbCommand.Append( "\"" );
			sbCommand.Append( tbFromPath.Text );
			sbCommand.Append( "\" " );
			sbCommand.Append( "\"" );
			sbCommand.Append( tbToPath.Text );
			sbCommand.Append( folder );
			sbCommand.Append( "\" " );

			int num = 0;
			if ( cbRandomFiles.IsChecked.Value && int.TryParse( tbRandomFileNum.Text, out num ) ) {
				string[] files = System.IO.Directory.GetFiles( tbFromPath.Text );

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

			if( cbE.IsChecked.Value ) { // 하위 디렉터리 복사
				sbCommand.Append( "/e " );
			}

			sbCommand.Append( "/copy:DAT " );

			if ( cbDcopyT.IsChecked.Value ) { // 타임스탬프 유지
				sbCommand.Append( "/dcopy:T " );
			}

			if( cbRetryNum.IsChecked.Value ) { // 실패한 복사에 대한 재시도 횟수 지정
				sbCommand.Append( "/r:" );
				sbCommand.Append( tbRetryNum.Text );
				sbCommand.Append( " " );
			}

			sbCommand.Append( "/mt" );
			sbCommand.Append( Environment.NewLine );

			process.Start();
			process.StandardInput.Write( sbCommand.ToString() );
			process.StandardInput.Close();

			Task task = CmdReader( process.StandardOutput );

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

		private async Task CmdReader( TextReader reader ) {
			string str;

			while ( ( str = await reader.ReadLineAsync() ) != null ) {
				MessageBox.Show( str );
			}
		}
	}
}

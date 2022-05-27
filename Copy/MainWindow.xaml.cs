using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
		private ProgressWnd             progressWnd     = new ProgressWnd();


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

			isRandomFiles.IsChecked = false;
			randomFileNum.IsEnabled = false;
		}

		private void OnBtnSrc( object sender, RoutedEventArgs e ) {
			if( fileDialog.ShowDialog() == CommonFileDialogResult.Ok ) {
				fromPath.Text = fileDialog.FileName;
			}
		}

		private void OnBtnDest( object sender, RoutedEventArgs e ) {
			if ( fileDialog.ShowDialog() == CommonFileDialogResult.Ok ) {
				toPath.Text = fileDialog.FileName;
			}
		}

		private void OnBtnCopy( object sender, RoutedEventArgs e ) {
			int lastIndex = fromPath.Text.LastIndexOf( '\\' );
			string folder = fromPath.Text.Substring( lastIndex, fromPath.Text.Length - lastIndex );

			sbCommand.Clear();
			sbCommand.Append( @"robocopy ");
			sbCommand.Append( "\"" );
			sbCommand.Append( fromPath.Text );
			sbCommand.Append( "\" " );
			sbCommand.Append( "\"" );
			sbCommand.Append( toPath.Text );
			sbCommand.Append( folder );
			sbCommand.Append( "\" " );

			int num = 0;
			if ( isRandomFiles.IsChecked.Value && int.TryParse( randomFileNum.Text, out num ) ) {
				string[] files = System.IO.Directory.GetFiles( fromPath.Text );

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

			if( isIncludeSubdir.IsChecked.Value ) { // 하위 디렉터리 복사
				sbCommand.Append( "/e " );
			}

			sbCommand.Append( "/copy:DAT " );

			if ( isDcopyTimeStamp.IsChecked.Value ) { // 타임스탬프 유지
				sbCommand.Append( "/dcopy:T " );
			}

			if( isRetryNum.IsChecked.Value ) { // 실패한 복사에 대한 재시도 횟수 지정
				sbCommand.Append( "/r:" );
				sbCommand.Append( retryNum.Text );
				sbCommand.Append( " " );
			}

			sbCommand.Append( "/mt" );
			sbCommand.Append( Environment.NewLine );

			process.Start();
			process.StandardInput.Write( sbCommand.ToString() );
			process.StandardInput.Close();

			progressWnd.Show( process );

			//process.WaitForExit();
			//process.Close();

			//MessageBox.Show( "작업 완료", "Robocopy" );
		}

		private void OnCheckRetryNum( object sender, RoutedEventArgs e ) {
			retryNum.IsEnabled = isRetryNum.IsChecked.Value;
		}

		private void OnUncheckRetryNum( object sender, RoutedEventArgs e ) {
			retryNum.IsEnabled = isRetryNum.IsChecked.Value;
		}

		private void OnCheckRandomFiles( object sender, RoutedEventArgs e ) {
			randomFileNum.IsEnabled = isRandomFiles.IsChecked.Value;
		}

		private void OnUncheckRandomFile( object sender, RoutedEventArgs e ) {
			randomFileNum.IsEnabled = isRandomFiles.IsChecked.Value;
		}

		/*
		private async Task CmdReader( TextReader reader ) {
			progressWnd.ShowDialog();

			string str;
			while ( ( str = await reader.ReadLineAsync() ) != null ) {
				progressWnd.progressText.Text += str;
			}

			process.Close();
			progressWnd.Close();
		}
		*/
	}
}

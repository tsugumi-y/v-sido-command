using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace command
{
	public partial class Form1 : Form
	{
		short[] deg = new short[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		Boolean[] mode = new Boolean[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
		public Form1()
		{
			InitializeComponent();
			//初期角度挿入
			foreach (Control c in this.idBox.Controls)
			{
				if (c is TextBox)
				{
					c.Text = deg[Int16.Parse((string)c.Tag) - 1].ToString("D");
				}
			}
			//　コントロールの順番設定
			this.idBox.Controls.SetChildIndex(id1, 0);
			this.idBox.Controls.SetChildIndex(id2, 1);
			this.idBox.Controls.SetChildIndex(id3, 2);
			this.idBox.Controls.SetChildIndex(id4, 3);
			this.idBox.Controls.SetChildIndex(id5, 4);
			this.idBox.Controls.SetChildIndex(id6, 5);
			this.idBox.Controls.SetChildIndex(id7, 6);
			this.idBox.Controls.SetChildIndex(id8, 7);
			this.idBox.Controls.SetChildIndex(id9, 8);
			this.idBox.Controls.SetChildIndex(id10, 9);
			this.idBox.Controls.SetChildIndex(id11, 10);
			this.idBox.Controls.SetChildIndex(id12, 11);
			this.idBox.Controls.SetChildIndex(id13, 12);
			this.idBox.Controls.SetChildIndex(id14, 13);
			this.idBox.Controls.SetChildIndex(id15, 14);
			this.idBox.Controls.SetChildIndex(id16, 15);

		}

		// チェックボックスを付けたServo IDのみ編集可能
		private void id_CheckedChanged(object sender, EventArgs e)
		{
			foreach (Control c in this.idBox.Controls)
			{
				if (c.Tag.Equals(((Control)sender).Tag) && (c is TextBox | c is Panel))
				{
					c.Enabled = ((CheckBox)sender).Checked;
				}
			}
		}

		// ラジオボタンによる角度指定方法の変更
		private void radiobutton_CheckedChanged(object sender, EventArgs e)
		{
			var radioButton = (RadioButton)sender;
			if (radioButton.Checked)
			{
				mode[Int16.Parse((string)radioButton.Tag) - 1] = (radioButton.Text == "Relative");
			}
		}


		// テキストボックスの入力チェック（符号つき数値またはbackSpace）
		private void TextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '-')
			{
				e.Handled = true;
			}
		}

		// Create Command ボタンのクリックイベント
		private void createBtn_Click(object sender, EventArgs e)
		{
			var commandArray = new List<byte>();
			string commandStr = string.Empty;
			byte servo_cnt = 0x00;
			byte cycle_time = 0x00;
			byte[] degArray;
			byte csum = 0x00;
			int index;

			// パケット開始
			commandArray.Add(0xff);

			// オペランド
			commandArray.Add(0x6f);

			// バイト長
			foreach (Control c in this.idBox.Controls)
			{
				CheckBox cb = c as CheckBox;
				if (cb != null && cb.Checked)
				{
					servo_cnt++;
				}
			}
			servo_cnt = (byte)(3 * servo_cnt + 5);
			commandArray.Add(servo_cnt);

			// 実行サイクル数
			if (!timeTxt.Text.Equals(string.Empty))
			{
				cycle_time = byte.Parse(timeTxt.Text);
				commandArray.Add(cycle_time);
			}
			else
			{
				MessageBox.Show("実行サイクル数(Time)を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
				goto Error;
			}

			// 角度取得(バグ：空白でもバイト長やID変化あり)
			foreach (Control c1 in this.idBox.Controls)
			{
				CheckBox cb = c1 as CheckBox;
				if (cb != null && cb.Checked)
				{
					commandArray.Add(byte.Parse((string)cb.Tag));
					foreach (Control c2 in this.idBox.Controls)
					{
						// テキストボックス内容処理
						if (c2.Tag.Equals(c1.Tag) && c2 is TextBox && !c2.Text.Equals(string.Empty))
						{
							index = Int16.Parse((string)c2.Tag) - 1;
							//Relative
							if (mode[index])
							{
								deg[index] += Int16.Parse(c2.Text);
							}
							//Absolute
							else
							{
								deg[index] = Int16.Parse(c2.Text);
							}
							degArray = GetByteDeg(deg[index]);
							c2.Text = deg[index].ToString("D");
							commandArray.Add(degArray[0]);
							commandArray.Add(degArray[1]);
						}
					}
				}
			}


			// チェックサム
			for (int cnt = 0; cnt < commandArray.Count; cnt++)
			{
				csum ^= commandArray[cnt];
			}
			commandArray.Add(csum);

			// 表示
			foreach (byte tmp in commandArray)
			{
				commandStr += string.Format("{0,0:x2}", tmp) + " ";

			}
			commandTxt.Text = commandStr;

			// エラー
			Error:;
		}

		// 角度の変換
		public byte[] GetByteDeg(short deg)
		{
			byte[] btdeg = { 0x00, 0x00 };
			int tmp1 = 0xff00;// 上位bit
			int tmp2 = 0x00ff;// 下位bit

			deg *= 10;
			deg <<= 1;
			tmp1 &= deg;
			tmp1 >>= 8;
			tmp1 <<= 1;
			tmp2 &= deg;
			btdeg[0] = (byte)tmp2;
			btdeg[1] = (byte)tmp1;

			return (byte[])btdeg.Clone();
		}

		// CSVファイルを選択
		private void openBtn_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			openFileDialog.Title = "CSV File Open";
			openFileDialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
			openFileDialog.FileName = "NewCommand";
			openFileDialog.Filter = "CSVファイル|*.csv";

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				filePath.Text = openFileDialog.FileName;
			}

			openFileDialog.Dispose();
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			if (filePath.Text.Equals(string.Empty))
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.FileName = "command.csv";
				saveFileDialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
				saveFileDialog.Filter = "CSVファイル|*.csv";
				saveFileDialog.Title = "Save New File";

				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					filePath.Text = saveFileDialog.FileName;
				}
				saveFileDialog.Dispose();
			}

			System.IO.StreamWriter SWriter = null;

			// 書き込み（末尾に追加）
			try
			{
				System.Text.Encoding EncoObj = System.Text.Encoding.GetEncoding("shift_jis");

				using (System.IO.FileStream fs = new System.IO.FileStream(filePath.Text, System.IO.FileMode.Append))
				{
					SWriter = new System.IO.StreamWriter(fs, EncoObj);

					string CsvLineStr;
					if (commandTxt.Text != string.Empty)
					{
						string[] stArrayData = commandTxt.Text.Split(' ');
						CsvLineStr = string.Join(",", stArrayData);
						CsvLineStr = CsvLineStr.TrimEnd(',');
						SWriter.WriteLine(CsvLineStr);
						SWriter.Close();
						SWriter = null;
						MessageBox.Show("保存しました。");
					}
					else
					{
						MessageBox.Show("コマンド未作成です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
						SWriter.Close();
						SWriter = null;
					}
				}
			}
			catch (System.IO.FileNotFoundException ex)
			{
				MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				if (SWriter != null) SWriter.Close();
			}
		}
	}
}

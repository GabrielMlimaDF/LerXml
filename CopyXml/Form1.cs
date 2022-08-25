using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CopyXml
{
    public partial class Form1 : Form
    {

        private static CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token = source.Token;


        public Form1()
        {
            InitializeComponent();
        }

        private void abrirdialog_HelpRequest(object sender, EventArgs e)
        {

        }

        private void Btnorigem_Click(object sender, EventArgs e)
        {
            if (abrirdialog.ShowDialog() == DialogResult.OK)
            {
                txtorigem.Text = abrirdialog.SelectedPath;
            }
        }

        private void Btndestino_Click(object sender, EventArgs e)
        {
            if (abrirdialog.ShowDialog() == DialogResult.OK)
            {
                txtdestino.Text = abrirdialog.SelectedPath;
            }
        }

        private async void Btniniciar_Click(object sender, EventArgs e)
        {
            var inicio = DateTime.Now;
            
            var cnpj = "00461552000105";
            if (txtorigem.Text == string.Empty)
            {
                MessageBox.Show("Campo origem deve ser preenchidos!", "Atenção: ", MessageBoxButtons.OK);

                txtorigem.BackColor = Color.Yellow;

                Btniniciar.Enabled = false;

            }

            if (txtdestino.Text == string.Empty)
            {
                MessageBox.Show("Campo destino deve ser preenchidos!", "Atenção: ", MessageBoxButtons.OK);
                txtdestino.BackColor = Color.Yellow;
                Btniniciar.Enabled = false;
            }


            string sourceDir = txtorigem.Text;
            string backupDir = txtdestino.Text;


            StatusBuscar status = new StatusBuscar();
            status.Show();


            Task<string[]> task = Task<string[]>.Factory.StartNew(() =>
            {

                var xmlfile = Directory.GetFiles(sourceDir, "*.xml", SearchOption.AllDirectories);
                return xmlfile;

            });

            await task;


            status.Visible = false;
            label3.Visible = true;
            Btncancelar.Enabled = true;
            Btniniciar.Enabled = false;
            ;


            var cont = 0;
            foreach (string f in task.Result)
            {
                try
                {

                    //Chamando CancellationToken
                    if (token.IsCancellationRequested)
                    {
                        MessageBox.Show("Operação cancelada. Sistema será fechado!", "Aviso");
                        this.Close();
                        break;
                    }


                    await Task.Delay(100);

                    //Colocando o nome do arquivo no split \\ 
                    string fName = f.Substring(sourceDir.Length + 1);
                    var cam = fName.Split('\\');

                    //Nome do arquivo separado do diretório
                    var subpasta = cam[cam.Length - 1];

                    //Add na listbox e mostrando status de copy
                    ListBoxStatus.Items.Add(subpasta);
                    ListBoxStatus.Items.Add(" Buscando CNPJ no arquivo xml...");
                    ListBoxStatus.Refresh();
                    cont++;
                    ListBoxStatus.Items.Add(cont);
                    //Carregar e ler Xml
                    XmlDocument doc = new XmlDocument();
                    doc.Load($@"{f}");
                    XmlNodeList xmlDoc = doc.GetElementsByTagName("emit");
                    for (int i = 0; i < xmlDoc.Count; i++)
                    {
                        //percorre o nó emit do xml e encontrar o nó abaixo
                        XmlNodeList xmlListItens = xmlDoc[i]["CNPJ"].ChildNodes;
                        string cnpjId = xmlDoc[i]["CNPJ"].ChildNodes[0].InnerText;

                        if (cnpjId == cnpj)
                        {
                            try
                            {
                                //Copiando os arquivos passados no subdiretorios
                                File.Copy(Path.Combine(sourceDir, fName), Path.Combine(backupDir, subpasta), true);
                            }
                            catch (IOException erro)
                            {
                                MessageBox.Show("Algo deu errado ao tentar copiar este arquivo!" + f + " " + erro.Message, "Aviso");

                            }

                        }

                    }

                }

                catch (Exception erro)
                {
                    MessageBox.Show("Algo deu errado ao tentar copiar este arquivo!" + f + " " + erro.Message, "Aviso");

                }
            }
            MessageBox.Show("Operação finalizada com sucesso! Arquivos copiados para: " + backupDir);

        }


        private void Btncancelar_Click(object sender, EventArgs e)
        {
            source.Cancel();


        }

    }


}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic;
using System.Globalization;


namespace Resolucion_error_MOV_CC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
        }
        static string archivo_auxiliar;
        static string mov_cc;
        int cancelar = 0;
        string archivo_nuevo;
        int linea_cancelacion;
        int lineCount;
        BackgroundWorker worker;
        //  fecha_llena | fecha_auxiliar        
        static Dictionary<string, string> fechas = new Dictionary<string, string>();

        static Dictionary<string, int> ocurrencias = new Dictionary<string, int>();


        private static bool EsNumerico(string s)
        {
            return int.TryParse(s, out int n);
        }
        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                { 
                    archivo_auxiliar = openFileDialog.FileName;

                    using (StreamReader fs = new StreamReader(archivo_auxiliar))
                    {
                        string line;
                        string[] aux;
                        char[] letras = { 'a', 'b', 'c', 'd', 'e', 'f',
                            'g', 'h', 'i', 'j', 'k', 'l',
                            'm', 'n', 'r', 'o', 'p', 'q',
                            's', 't', 'u', 'v', 'w', 'x',
                            'y', 'z', 'A', 'B', 'C', 'D',
                       'E', 'F', 'G', 'H', 'I', 'J',
                        'K', 'L', 'M', 'N', 'R', 'O',
                        'P', 'Q', 'S', 'T', 'U', 'V',
                        'W', 'X', 'Y', 'Z'
                        };
                        if(fechas.Count > 0 && ocurrencias.Count > 0)
                        {
                            fechas.Clear();
                            ocurrencias.Clear();
                        }
                        if(dataGridView1.Rows.Count > 0)
                        {
                            dataGridView1.Rows.Clear();
                        }
                        int fechas__invalidas = 0;
                        bool cancelar = false;
                        while ((line = fs.ReadLine()) != null)
                        {
                            aux = line.Split(' ');
                            if (aux[0].IndexOfAny(letras) == -1 && aux[1].IndexOfAny(letras) == -1)
                            {
                                if(!fechas.ContainsKey(aux[0]))
                                {
                                    fechas.Add(aux[0], aux[1]);
                                    ocurrencias.Add(aux[0], 0);
                                    dataGridView1.Rows.Add(aux[0], aux[1], "0");
                                    dataGridView1.Refresh();
                                } else
                                {
                                    MessageBox.Show("Excepcion: ya existe una fecha con el valor " + aux[0] + " : " + aux[1] + " en el archivo de fechas");
                                }
                            }
                            else
                            {
                                if(fechas__invalidas >= 5)
                                {
                                    var result = MessageBox.Show("Se han detectado demasiadas fechas inválidas, ¿desea reintentar la carga?", 
                                        "Alerta",
                                        MessageBoxButtons.YesNo,MessageBoxIcon.Warning);
                                    if(result == DialogResult.Yes)
                                    {
                                        fechas.Clear();
                                        dataGridView1.Rows.Clear();
                                        dataGridView1.Refresh();
                                        cancelar = true;
                                        fs.Close();
                                        break;
                                    }
                                    else if (result == DialogResult.No)
                                    {
                                        fechas__invalidas = 0;
                                        continue;
                                    }
                                }
                                fechas__invalidas++;
                                MessageBox.Show("Fecha inválida: " + aux[0] + " | " + aux[1],"ERROR", MessageBoxButtons.OK);
                                continue;
                            }
                        }
                        if(cancelar == false)
                        {
                            label2.Text = "Se cargaron un total de " + fechas.Count + " fechas.";
                            button2.Enabled = true;
                        }

                    }
                } else MessageBox.Show("Debes cargar un archivo con fechas para poder continuar!", "ERROR", MessageBoxButtons.OK);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;
                DialogResult r = openFileDialog.ShowDialog();
                if (r == DialogResult.OK)
                {
                    mov_cc = openFileDialog.FileName;
                    MessageBox.Show("Archivo cargado correctamente!");
                    button3.Enabled = true;
                }
                else if(r == DialogResult.Cancel)
                {
                    return;
                }
                else MessageBox.Show("Archivo invalido.");
            }
        }
        DateTime startTime, endTime;

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(cancelar == 0)
            {
                endTime = DateTime.Now;
                Double elapsedMillisecs = ((TimeSpan)(endTime - startTime)).TotalSeconds;
                MessageBox.Show("Se ha procesado el archivo " + mov_cc  + "\nTiempo transcurrido: " + Math.Round(elapsedMillisecs, MidpointRounding.ToEven).ToString() + " segundos");
            }
            else if(cancelar == 1)
            {
                
                File.Delete(archivo_nuevo);
                MessageBox.Show("Operación Cancelada, archivo de salida eliminado.");
                cancelar = 0;
            }

            progressBar1.Value = 0;
            button3.Enabled = false;
            button2.Enabled = false;
            button1.Enabled = true;
            button4.Enabled = false;
            label2.Text = "";
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {



            using (StreamReader fs = new StreamReader(mov_cc))
            {

                string line;
                string linea;
                string linea_temporal;
                string linea_temporal_bis;
                string _fecha;
                string _fecha2;
                int contador = 0;
                int contador_2 = 0;
                double porcentaje = 0;

                for (int i = 0; i != linea_cancelacion; i++)
                {
                    fs.ReadLine();
                    contador_2++;
                    Invoke(new Action(() =>
                    {
                        progressBar1.PerformStep();
                        label7.Text = Math.Round((double)(contador_2 / progressBar1.Maximum) * 100).ToString() + "%";
                        label7.Update();
                    }));
                }

                using (StreamWriter new_file = new StreamWriter(archivo_nuevo))
                {
                    var list_fechas = fechas.Keys.ToList();
                    startTime = DateTime.Now;
                    while ((line = fs.ReadLine()) != null && cancelar == 0)
                    {

                        contador_2++;
                        linea = line;
                        linea_temporal = linea.Substring(30);
                        linea_temporal_bis = linea.Remove(30);
                        _fecha = linea_temporal.Substring(0, 8);
                        _fecha2 = _fecha;
                        if (fechas.ContainsKey(_fecha))
                        {
                            ocurrencias[_fecha]++;
                            contador++;
                            /*Invoke(new Action(() =>
                            {
                                label10.Text = contador.ToString();
                                label10.Update();
                                try
                                {
                                    dataGridView1.Rows[fechas.Keys.ToList().IndexOf(_fecha)].Cells[2].Value = ocurrencias[_fecha];

                                }
                                catch (Exception e3)
                                {
                                    MessageBox.Show(e3.Message);
                                }
                                dataGridView1.Refresh();
                            }));*/
                            if (ocurrencias[_fecha] > 15000)
                            {

                                string _newdate = fechas[_fecha];
                                _newdate = _newdate.Substring(0, 4) + "/" + _newdate.Substring(4, 2) + "/" + _newdate.Substring(6);
                                DateTime date;
                                DateTime.TryParse(_newdate, out date);
                                var nextdate = date.AddDays(1);
                                string newday, newmonth;
                                if (nextdate.Day.ToString().Length == 1)
                                {
                                    newday = "0" + nextdate.Day.ToString();
                                }
                                else newday = nextdate.Day.ToString();

                                if (nextdate.Month.ToString().Length == 1)
                                {
                                    newmonth = "0" + nextdate.Month.ToString();
                                }
                                else newmonth = nextdate.Month.ToString();

                                _newdate = nextdate.Year.ToString() + newmonth + newday;

                                fechas[_fecha] = _newdate;
                                ocurrencias[_fecha] = 0;
                                MessageBox.Show("Se han reemplazado más de 15000 ocurrencias para la fecha " + _fecha + " Se reemplaza con el día siguiente (" + fechas[_fecha] + ")");
                                dataGridView1.Rows[list_fechas.IndexOf(_fecha2)].Cells[1].Value = fechas[_fecha];
                            }
                            _fecha = _fecha.Replace(_fecha, fechas[_fecha]);
                        }
                        porcentaje = Math.Round(Convert.ToDouble(contador_2) / Convert.ToDouble(lineCount) * 100, 1, MidpointRounding.AwayFromZero);
                        Invoke(new Action(() =>
                        {
                            label8.Text = contador_2.ToString();
                            label7.Text = porcentaje.ToString() + "%";
                            label7.Update();
                            label8.Update();
                            progressBar1.PerformStep();
                            label10.Text = contador.ToString();
                            label10.Update();

                            if(list_fechas.IndexOf(_fecha2) > -1)
                            {
                                dataGridView1.Rows[list_fechas.IndexOf(_fecha2)].Cells[2].Value = ocurrencias[_fecha2];
                            }                   
                        }));
                        /*
                        foreach (KeyValuePair<string, string> entry in fechas)
                        {
                            if (_fecha.Contains(entry.Key))
                            {

                                _fecha = _fecha.Replace(entry.Key.ToString(), entry.Value.ToString());
                                contador++;
                                Invoke(new Action(() =>
                                {
                                    label10.Text = contador.ToString();
                                    label10.Update();
                                }));

                                break;
                            }
                            else continue;
                        }
                        */

                        linea_temporal = linea_temporal.Substring(8);
                        linea = String.Concat(linea_temporal_bis, _fecha, linea_temporal);
                        /*
                        using (StreamWriter sw = File.AppendText(archivo_nuevo))
                        {
                            sw.WriteLine(linea);
                        }*/
                        new_file.WriteLine(linea);
                    }
                    new_file.Close();
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        { 
            __linea:
            {
                string input = Interaction.InputBox("Ingrese el número de línea dónde canceló el conversor:", "Línea");
                linea_cancelacion = 0;
                
                if (EsNumerico(input))
                {
                    button3.Enabled = false;
                    button2.Enabled = false;
                    button4.Enabled = true;
                    button1.Enabled = false;
                    linea_cancelacion = Int32.Parse(input);
                    archivo_nuevo = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\mov-cc.dat";
                    
                    if(File.Exists(archivo_nuevo))
                    {
                        archivo_nuevo = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + ".dat";
                    }
                    worker = new BackgroundWorker { WorkerReportsProgress = true };

                    worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                    worker.RunWorkerAsync();
                    lineCount = File.ReadLines(mov_cc).Count();
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = lineCount;
                    progressBar1.Step = 1;

                }
                else if(input.Length == 0)
                {
                    return;
                }
                else 
                {
                    MessageBox.Show("Ingrese un número válido!");
                    goto __linea;
                }
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            cancelar = 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

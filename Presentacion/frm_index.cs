using TP_Final.Modelo;
using System;
using System.Data;
using System.Windows.Forms;

namespace TP_Final.Presentacion
{
    public partial class frm_index : Form
    {
        public frm_index()
        {
            InitializeComponent();
        }
        private void frm_principal_Load(object sender, EventArgs e)
        {
            btnRestablecer.Enabled = false;
        }

        private bool validateParameters()
        {
            if (nudCantSim.Value == 0)
            {
                MessageBox.Show("La cantidad de simulaciones a generar debe ser mayor a 0.", "Generación de Simulación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (nudSimDesde.Value > nudCantSim.Value)
            {
                MessageBox.Show("El valor ingresado desde donde mostrar debe ser menor a la cantidad de Simulaciones a generar.", "Generación de Simulación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (nudDiasRevision.Value == 0)
            {
                MessageBox.Show("La cantidad de días entre revisiones debe ser mayor a 0.", "Generación de Simulación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (Math.Round((double)nudProb4dias.Value + (double)nudProb5dias.Value + (double)nudProb6dias.Value + (double)nudProb7dias.Value, 2) != 1)
            {
                MessageBox.Show("La suma de probabilidades debe igual a 1.", "Probabilidades de Avería", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void numActive(object sender, EventArgs e)
        {
            NumericUpDown clickedNumeric = (NumericUpDown)sender;
            clickedNumeric.Select(0, clickedNumeric.Text.Length);
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            if (validateParameters())
            {
                //Creación de objetos Mantenimiento
                PoliticasAyB mPreventivo = new PoliticasAyB((double)nudCostoArreglo.Value, (double)nudCostoRevision.Value, (int)nudDiasRevision.Value, true, (double)nudProb4dias.Value, (double)nudProb5dias.Value, (double)nudProb6dias.Value, (int)nudSimDesde.Value);
                PoliticasAyB mCorrectivo = new PoliticasAyB((double)nudCostoArreglo.Value, (double)nudProb4dias.Value, (double)nudProb5dias.Value, (double)nudProb6dias.Value, (int)nudSimDesde.Value);

                //Generación de experimentos y pedido de tablas
                mPreventivo.calcularExperimento((int)nudCantSim.Value);
                mCorrectivo.calcularExperimento((int)nudCantSim.Value);
                dgvTablaPreventivo.DataSource = mPreventivo.getTabla();
                dgvTablaCorrectivo.DataSource = mCorrectivo.getTabla();
                dgvPreventivoFinal.DataSource = mPreventivo.getTablaUltimaFila();
                dgvCorrectivoFinal.DataSource = mCorrectivo.getTablaUltimaFila();
                mostrarMetricas(mPreventivo, mCorrectivo);

                gbParametros.Enabled = false;
                gbProbabilidades.Enabled = false;
                btnRestablecer.Enabled = true;
                btnGenerar.Enabled = false;
                
                gbResultado.Visible = true;
                lblResultado.Text = generarResultado();
            }
        }

        private void mostrarMetricas(PoliticasAyB preventivo, PoliticasAyB correctivo)
        {
            //Mantenimiento Correctivo
            lblCantAveriasCorrectivoValue.Text = correctivo.ContadorAverias.ToString();
            lblGastoAveriasCorrectivoValue.Text = correctivo.GastosAverias.ToString();
            lblPromDiasAveriaCorrectivoValue.Text = Math.Round(correctivo.PromedioDiasAverias, 5).ToString();

            //Mantenimiento Preventivo
            lblCantAveriasPreventivoValue.Text = preventivo.ContadorAverias.ToString();
            lblCantRevisionesValue.Text = preventivo.ContadorRevisiones.ToString();
            lblGastoAveriasPreventivoValue.Text = preventivo.GastosAverias.ToString();
            lblGastoRevisionesValue.Text = preventivo.GastosRevisiones.ToString();
            lblPromDiasAveriasPreventivoValue.Text = Math.Round(preventivo.PromedioDiasAverias, 5).ToString();
        }

        private void borrarMetricas()
        {
            //Mantenimiento Correctivo
            lblCantAveriasCorrectivoValue.Text = "0";
            lblGastoAveriasCorrectivoValue.Text = "0";
            lblPromDiasAveriaCorrectivoValue.Text = "0";

            //Mantenimiento Preventivo
            lblCantAveriasPreventivoValue.Text = "0";
            lblCantRevisionesValue.Text = "0";
            lblGastoAveriasPreventivoValue.Text = "0";
            lblGastoRevisionesValue.Text = "0";
            lblPromDiasAveriasPreventivoValue.Text = "0";
        }

        private void btnRestablecer_Click(object sender, EventArgs e)
        {
            nudCantSim.Focus();
            dgvTablaCorrectivo.Columns.Clear();
            dgvCorrectivoFinal.Columns.Clear();
            dgvTablaPreventivo.Columns.Clear();
            dgvPreventivoFinal.Columns.Clear();
            gbParametros.Enabled = true;
            gbProbabilidades.Enabled = true;
            borrarMetricas();
            btnRestablecer.Enabled = false;
            btnGenerar.Enabled = true;
            gbResultado.Visible = false;
        }

        private string generarResultado()
        {
            string resultado = "La estrategia que conviene utilizar para el Mantenimiento es el ";
            if (Convert.ToDouble(dgvCorrectivoFinal.Rows[0].Cells[dgvCorrectivoFinal.ColumnCount - 1].Value.ToString()) > Convert.ToDouble(dgvPreventivoFinal.Rows[0].Cells[dgvPreventivoFinal.ColumnCount - 1].Value.ToString()))
            {
                resultado += "Preventivo.";
            }
            else
            {
                resultado += "Correctivo.";
            }
            return resultado;
        }
    }
}

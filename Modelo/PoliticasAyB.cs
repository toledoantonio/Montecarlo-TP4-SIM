using System;
using System.Collections.Generic;
using System.Data;

namespace TP_Final.Modelo
{
    internal class PoliticasAyB
    {
        //Atributos
        private double costArreglo, costRevision;
        private int diasParaRevision;
        private bool esPreventivo;
        private int contadorAverias, contadorRevisiones;
        private uint acumuladorAverias;
        private double gastosAverias, gastosRevisiones, promedioDiasAverias;
        private Random generador;
        private Fila fila, filaAnterior;
        double probabilidad1, probabilidad2, probabilidad3, probabilidad4;
        DataTable tabla, tablaUltimaFila;
        int mostrarDesde;

        //Constructor Mantenimiento Correctivo
        public PoliticasAyB(double ka, double p1, double p2, double p3, int mostrarDesde)
        {
            this.costArreglo = ka;
            costRevision = 0;
            diasParaRevision = 0;
            esPreventivo = false;
            this.probabilidad1 = p1;
            this.probabilidad2 = probabilidad1 + p2;
            this.probabilidad3 = probabilidad2 + p3;
            this.generarTabla();
            this.generarTablaUltimaFila();
            this.mostrarDesde = mostrarDesde;
        }

        //Constructor Mantenimiento Preventivo
        public PoliticasAyB(double ka, double kr, int diasParaRevision, bool esPreventivo, double p1, double p2, double p3, int mostrarDesde)
        {
            this.costArreglo = ka;
            this.esPreventivo = esPreventivo;
            if (esPreventivo)
            {
                this.costRevision = kr;
                this.diasParaRevision = diasParaRevision;
            }
            this.probabilidad1 = p1;
            this.probabilidad2 = probabilidad1 + p2;
            this.probabilidad3 = probabilidad2 + p3;
            this.generarTabla();
            this.generarTablaUltimaFila();
            this.mostrarDesde = mostrarDesde;
        }

        //Getters y Setters
        public double GastosAverias { get => gastosAverias; set => gastosAverias = value; }
        public double GastosRevisiones { get => gastosRevisiones; set => gastosRevisiones = value; }
        public double PromedioDiasAverias { get => promedioDiasAverias; set => promedioDiasAverias = value; }
        public int ContadorAverias { get => contadorAverias; set => contadorAverias = value; }
        public int ContadorRevisiones { get => contadorRevisiones; set => contadorRevisiones = value; }
        public DataTable getTabla() {return this.tabla;}
        public DataTable getTablaUltimaFila() {return this.tablaUltimaFila;}

        //Cálculos de Simulación
        public void calcularExperimento(int N)
        {
            generador = new Random();

            this.filaAnterior = new Fila();
            fila = new Fila();

            ContadorAverias = 0;
            ContadorRevisiones = 0;
            acumuladorAverias = 0;
            GastosAverias = 0;
            GastosRevisiones = 0;
            PromedioDiasAverias = 0;
            int mostrarHasta = mostrarDesde + 400;

            for (int i = 1; i <= N; i++)
            {
                fila.Dia = filaAnterior.Dia + 1;
                fila.DiaAveria = filaAnterior.DiaAveria;
                fila.DiasParaAveria = filaAnterior.DiasParaAveria;
                fila.DiaRevision = filaAnterior.DiaRevision; // No hace falta checkear esPreventivo

                if (fila.Dia == fila.DiaAveria)
                {
                    fila.CostoArreglo = costArreglo;
                    ContadorAverias += 1; //Metrica
                    acumuladorAverias += (uint)fila.DiasParaAveria;
                }
                //Si es dia de revision, sumar costo de revision, siempre y cuando no haya habido arreglo de motor el día anterior
                else if (esPreventivo && fila.DiaRevision == fila.Dia && !(filaAnterior.DiaAveria == filaAnterior.Dia))
                {
                    fila.CostoRevision = costRevision;
                    ContadorRevisiones += 1; //Metrica
                }
                if (filaAnterior.Dia == filaAnterior.DiaAveria || esPreventivo && fila.Dia == fila.DiaRevision && !(filaAnterior.DiaAveria == filaAnterior.Dia))
                {
                    //Genero RND y seteo los nuevos dias para la próxima avería
                    fila.Rnd = generador.NextDouble();
                    fila.DiasParaAveria = calcularDiasPAveria(fila.Rnd);
                    fila.DiaAveria = fila.Dia + fila.DiasParaAveria;
                    //Renuevo los dias de revision porque el motor está arreglado
                    fila.DiasParaRevision = diasParaRevision;
                    fila.DiaRevision = fila.Dia + fila.DiasParaRevision;
                }

                // No hace falta checkear esPreventivo, si no lo es => costoRevision = 0 siempre
                fila.CostoTotal = fila.CostoArreglo + fila.CostoRevision;
                fila.CostoAcumulado = filaAnterior.CostoAcumulado + fila.CostoTotal;
                fila.PromedioXDia = fila.CostoAcumulado / (double)fila.Dia;

                //Agregar nueva fila a tabla cuando corresponda
                if (i >= this.mostrarDesde && i < mostrarHasta)
                {
                    agregarFilaTabla(fila.Dia, fila.Rnd, fila.DiasParaAveria, fila.DiaAveria, fila.DiasParaRevision, fila.DiaRevision, fila.CostoArreglo, fila.CostoRevision, fila.CostoTotal, fila.CostoAcumulado, fila.PromedioXDia);
                }
                //Última fila
                if (i == N)
                {
                    agregarUltimaFila(fila.Dia, fila.Rnd, fila.DiasParaAveria, fila.DiaAveria, fila.DiasParaRevision, fila.DiaRevision, fila.CostoArreglo, fila.CostoRevision, fila.CostoTotal, fila.CostoAcumulado, fila.PromedioXDia);
                }
                filaAnterior = fila;
                fila = new Fila();
            }

            //Cálculo de metricas
            GastosAverias = ContadorAverias * costArreglo;
            GastosRevisiones = ContadorRevisiones * costRevision;
            if (ContadorAverias != 0)
                PromedioDiasAverias = acumuladorAverias / (double)ContadorAverias;
        }

        public int calcularDiasPAveria(double rnd)
        {
            if (rnd < probabilidad1) return 5;
            if (rnd < probabilidad2) return 6;
            if (rnd < probabilidad3) return 7;
            return 8;
        }

        //Tabla
        private void generarTabla()
        {
            tabla = new DataTable();
            // cabecera 
            string[] columnasTXT = this.getColumnas();
            for (int i = 0; i < columnasTXT.Length; i++)
            {
                tabla.Columns.Add(columnasTXT[i]);
            }
        }

        private void generarTablaUltimaFila()
        {
            tablaUltimaFila = new DataTable();
            string[] columnasTXT = this.getColumnas();
            for (int i = 0; i < columnasTXT.Length; i++)
            {
                tablaUltimaFila.Columns.Add(columnasTXT[i]);
            }
        }

        public string[] getColumnas()
        {
            if (this.esPreventivo)
            {
                return new string[] { "Reloj (Días)", "RND", "Días faltantes avería", "Día avería", "Días faltantes revisión", "Día revisión",
                "Costo arreglo", "Costo revisión", "Costo Total", "Costo Ac", "Costo Prom por día"};
            }
            return new string[] { "Reloj (Días)", "RND", "Días faltantes avería", "Día avería", "Costo arreglo", "Costo Ac", "Costo Prom por día" };
        }
        
        private void agregarFilaTabla(int dia, double RND, int diasParaAveria, int diaAveria, int diasParaRevision, int diaRevision, double costoArreglo, double costoRevision, double costoTotal, double costoAC, double promedio)
        {
            if (this.esPreventivo)
            {
                this.tabla.Rows.Add(dia, RND != 0? RND.ToString() : "", diasParaAveria, diaAveria, diasParaRevision, diaRevision, costoArreglo, costoRevision, costoTotal, costoAC, promedio);
            }
            else
            {
                this.tabla.Rows.Add(dia, RND != 0 ? RND.ToString() : "", diasParaAveria, diaAveria, costoArreglo, costoAC, promedio);
            }
        }

        private void agregarUltimaFila(int dia, double RND, int diasParaAveria, int diaAveria, int diasParaRevision, int diaRevision, double costoArreglo, double costoRevision, double costoTotal, double costoAC, double promedio)
        {
            if (this.esPreventivo)
            {
                this.tablaUltimaFila.Rows.Add(dia, RND != 0 ? RND.ToString() : "", diasParaAveria, diaAveria, diasParaRevision, diaRevision, costoArreglo, costoRevision, costoTotal, costoAC, promedio);
            }
            else
            {
                this.tablaUltimaFila.Rows.Add(dia, RND != 0 ? RND.ToString() : "", diasParaAveria, diaAveria, costoArreglo, costoAC, promedio);
            }
        }
    }

    class Fila
    {
        int diasParaRevision = 0;
        int diaRevision = 0;
        double costoRevision = 0;
        double costoTotal = 0;

        int dia = 0;
        double rnd = 0;
        int diasParaAveria = 0;
        int diaAveria = 0;
        double costoArreglo = 0;
        double costoAcumulado = 0;
        double promedioXDia = 0;

        public int Dia { get => dia; set => dia = value; }
        public double Rnd { get => rnd; set => rnd = value; }
        public int DiasParaAveria { get => diasParaAveria; set => diasParaAveria = value; }
        public int DiaAveria { get => diaAveria; set => diaAveria = value; }
        public double CostoArreglo { get => costoArreglo; set => costoArreglo = value; }
        public double CostoAcumulado { get => costoAcumulado; set => costoAcumulado = value; }
        public double PromedioXDia { get => promedioXDia; set => promedioXDia = value; }

        public int DiasParaRevision { get => diasParaRevision; set => diasParaRevision = value; }
        public int DiaRevision { get => diaRevision; set => diaRevision = value; }
        public double CostoRevision { get => costoRevision; set => costoRevision = value; }
        public double CostoTotal { get => costoTotal; set => costoTotal = value; }
    }
}

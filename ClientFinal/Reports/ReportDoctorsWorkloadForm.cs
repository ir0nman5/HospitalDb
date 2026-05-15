using HospitalDbClient.Data;
using Npgsql;
using System.Data;
using System.Linq;

namespace HospitalDbClient.Reports;

public sealed class ReportDoctorsWorkloadForm : Form
{
    private readonly DataGridView _grid = new();

    private readonly NumericUpDown _numMinPatients = new();

    private readonly ComboBox _cmbSort = new();

    private readonly Label _lblTotal = new();

    public ReportDoctorsWorkloadForm()
    {
        Text = "Отчёт: нагрузка врачей";

        Width = 1300;
        Height = 750;

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(10)
        };

        _numMinPatients.Width = 80;
        _numMinPatients.Minimum = 0;
        _numMinPatients.Maximum = 100;

        _cmbSort.Width = 180;

        _cmbSort.Items.AddRange(new[]
        {
            "Всего пациентов",
            "Активных пациентов",
            "ФИО врача"
        });

        _cmbSort.SelectedIndex = 0;

        var btnBuild = new Button
        {
            Text = "Сформировать",
            Width = 150,
            Height = 35
        };

        btnBuild.Click += (_, _) => LoadReport();

        top.Controls.Add(new Label
        {
            Text = "Минимум пациентов:",
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0)
        });

        top.Controls.Add(_numMinPatients);

        top.Controls.Add(new Label
        {
            Text = "Сортировка:",
            AutoSize = true,
            Padding = new Padding(15, 8, 0, 0)
        });

        top.Controls.Add(_cmbSort);

        top.Controls.Add(btnBuild);

        top.Controls.Add(_lblTotal);

        _grid.Dock = DockStyle.Fill;

        _grid.ReadOnly = true;

        _grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        Controls.Add(_grid);
        Controls.Add(top);
    }

    private void LoadReport()
    {
        string orderBy = _cmbSort.SelectedIndex switch
        {
            0 => "total_patients DESC",
            1 => "active_patients DESC",
            _ => "doctor_name ASC"
        };

        var table = Db.GetTable($"""
            SELECT
                d.full_name AS doctor_name,
                p.name AS position_name,

                COUNT(pt.patient_id) AS total_patients,

                COUNT(*) FILTER
                (
                    WHERE pt.discharge_date IS NULL
                ) AS active_patients

            FROM doctors d

            JOIN positions p
                ON p.position_id = d.position_id

            LEFT JOIN patients pt
                ON pt.doctor_inn = d.doctor_inn

            GROUP BY
                d.full_name,
                p.name

            HAVING COUNT(pt.patient_id) >= @min

            ORDER BY {orderBy}
        """,
        new NpgsqlParameter("min",
            (int)_numMinPatients.Value));

        _grid.DataSource = table;

        int total =
            table.AsEnumerable()
                .Sum(r => Convert.ToInt32(r["total_patients"]));

        _lblTotal.Text =
            $"Всего пациентов в отчёте: {total}";
    }
}
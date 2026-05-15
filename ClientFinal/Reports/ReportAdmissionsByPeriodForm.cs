using HospitalDbClient.Data;
using Npgsql;
using System.Data;
using System.Linq;

namespace HospitalDbClient.Reports;

public sealed class ReportAdmissionsByPeriodForm : Form
{
    private readonly DataGridView _grid = new();

    private readonly DateTimePicker _dtFrom = new();

    private readonly DateTimePicker _dtTo = new();

    private readonly ComboBox _cmbSort = new();

    private readonly Label _lblTotal = new();

    public ReportAdmissionsByPeriodForm()
    {
        Text = "Отчёт: поступления по диагнозам";

        Width = 1300;
        Height = 750;

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 70,
            Padding = new Padding(10)
        };

        _dtFrom.Value = DateTime.Today.AddMonths(-1);

        _dtTo.Value = DateTime.Today;

        _cmbSort.Width = 180;

        _cmbSort.Items.AddRange(new[]
        {
            "Количество случаев",
            "Диагноз"
        });

        _cmbSort.SelectedIndex = 0;

        var btn = new Button
        {
            Text = "Сформировать",
            Width = 150,
            Height = 35
        };

        btn.Click += (_, _) => LoadReport();

        top.Controls.Add(new Label
        {
            Text = "Период с:",
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0)
        });

        top.Controls.Add(_dtFrom);

        top.Controls.Add(new Label
        {
            Text = "по:",
            AutoSize = true,
            Padding = new Padding(10, 8, 0, 0)
        });

        top.Controls.Add(_dtTo);

        top.Controls.Add(new Label
        {
            Text = "Сортировка:",
            AutoSize = true,
            Padding = new Padding(10, 8, 0, 0)
        });

        top.Controls.Add(_cmbSort);

        top.Controls.Add(btn);

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
            0 => "admissions_count DESC",
            _ => "diagnosis_name ASC"
        };

        var table = Db.GetTable($"""
            SELECT
                d.name AS diagnosis_name,

                COUNT(p.patient_id)
                    AS admissions_count,

                COUNT(*) FILTER
                (
                    WHERE p.discharge_date IS NULL
                ) AS active_cases

            FROM patients p

            JOIN diagnoses d
                ON d.diagnosis_id = p.diagnosis_id

            WHERE p.admission_date
                BETWEEN @from AND @to

            GROUP BY d.name

            HAVING COUNT(p.patient_id) > 0

            ORDER BY {orderBy}
        """,
        new NpgsqlParameter("from", _dtFrom.Value.Date),
        new NpgsqlParameter("to", _dtTo.Value.Date));

        _grid.DataSource = table;

        int total =
            table.AsEnumerable()
                .Sum(r => Convert.ToInt32(r["admissions_count"]));

        _lblTotal.Text =
            $"Всего поступлений: {total}";
    }
}
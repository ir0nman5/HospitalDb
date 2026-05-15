using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace HospitalDbClient.Data;

public abstract class CrudFormBase : Form
{
    protected abstract string TableName { get; }
    protected abstract string KeyColumn { get; }

    protected virtual string[] SearchColumns => [];

    protected virtual string DefaultOrderBy => KeyColumn;

    protected abstract List<FieldSpec> Fields { get; }

    private readonly DataGridView _grid = new();

    private readonly TextBox _txtSearch = new();

    private readonly ComboBox _cmbFilterField = new();

    private readonly TextBox _txtFilterValue = new();

    private readonly ComboBox _cmbSortField = new();

    private readonly CheckBox _chkDesc = new();

    private readonly BindingSource _binding = new();

    protected CrudFormBase(string title)
    {
        Text = title;

        Width = 1400;
        Height = 850;

        StartPosition = FormStartPosition.CenterParent;

        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 90,
            Padding = new Padding(10),
            AutoSize = false,
            WrapContents = true
        };

        _txtSearch.Width = 220;
        _txtSearch.PlaceholderText = "Поиск";

        _cmbFilterField.Width = 160;

        _txtFilterValue.Width = 180;
        _txtFilterValue.PlaceholderText = "Фильтр";

        _cmbSortField.Width = 160;

        _chkDesc.Text = "DESC";

        var btnLoad = new Button
        {
            Text = "Обновить",
            Width = 110,
            Height = 35
        };

        var btnAdd = new Button
        {
            Text = "Добавить",
            Width = 110,
            Height = 35
        };

        var btnEdit = new Button
        {
            Text = "Изменить",
            Width = 110,
            Height = 35
        };

        var btnDelete = new Button
        {
            Text = "Удалить",
            Width = 110,
            Height = 35
        };

        var btnFirst = new Button
        {
            Text = "|<",
            Width = 50,
            Height = 35
        };

        var btnPrev = new Button
        {
            Text = "<",
            Width = 50,
            Height = 35
        };

        var btnNext = new Button
        {
            Text = ">",
            Width = 50,
            Height = 35
        };

        var btnLast = new Button
        {
            Text = ">|",
            Width = 50,
            Height = 35
        };

        btnLoad.Click += (_, _) => LoadGrid();

        btnAdd.Click += (_, _) => AddRecord();

        btnEdit.Click += (_, _) => EditRecord();

        btnDelete.Click += (_, _) => DeleteRecord();

        btnFirst.Click += (_, _) => _binding.MoveFirst();

        btnPrev.Click += (_, _) => _binding.MovePrevious();

        btnNext.Click += (_, _) => _binding.MoveNext();

        btnLast.Click += (_, _) => _binding.MoveLast();

        _txtSearch.TextChanged += (_, _) => LoadGrid();

        _txtFilterValue.TextChanged += (_, _) => LoadGrid();

        _cmbFilterField.SelectedIndexChanged += (_, _) => LoadGrid();

        _cmbSortField.SelectedIndexChanged += (_, _) => LoadGrid();

        _chkDesc.CheckedChanged += (_, _) => LoadGrid();

        topPanel.Controls.AddRange(new Control[]
        {
            new Label
            {
                Text = "Поиск:",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            },

            _txtSearch,

            new Label
            {
                Text = "Фильтр:",
                AutoSize = true,
                Padding = new Padding(10, 8, 0, 0)
            },

            _cmbFilterField,

            _txtFilterValue,

            new Label
            {
                Text = "Сортировка:",
                AutoSize = true,
                Padding = new Padding(10, 8, 0, 0)
            },

            _cmbSortField,

            _chkDesc,

            btnLoad,
            btnAdd,
            btnEdit,
            btnDelete,

            btnFirst,
            btnPrev,
            btnNext,
            btnLast
        });

        _grid.Dock = DockStyle.Fill;

        _grid.ReadOnly = true;

        _grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        _grid.MultiSelect = false;

        _grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        Controls.Add(_grid);

        Controls.Add(topPanel);

        Shown += (_, _) =>
        {
            InitCombos();
            LoadGrid();
        };
    }

    private void InitCombos()
    {
        var searchable =
            SearchColumns.Length > 0
                ? SearchColumns
                : Fields
                    .Where(f =>
                        f.Kind == FieldKind.Text ||
                        f.Kind == FieldKind.Multiline)
                    .Select(f => f.Name)
                    .ToArray();

        _cmbFilterField.Items.Clear();

        _cmbSortField.Items.Clear();

        foreach (var f in searchable)
            _cmbFilterField.Items.Add(f);

        foreach (var f in Fields.Select(x => x.Name))
            _cmbSortField.Items.Add(f);

        if (_cmbFilterField.Items.Count > 0)
            _cmbFilterField.SelectedIndex = 0;

        if (_cmbSortField.Items.Count > 0)
        {
            var idx =
                _cmbSortField.Items.IndexOf(DefaultOrderBy);

            _cmbSortField.SelectedIndex =
                idx >= 0 ? idx : 0;
        }
    }

    protected virtual string BuildSelectSql(
        out List<NpgsqlParameter> parameters)
    {
        parameters = new List<NpgsqlParameter>();

        var where = new List<string>();

        if (!string.IsNullOrWhiteSpace(_txtSearch.Text))
        {
            var parts = new List<string>();

            foreach (var col in
                     (SearchColumns.Length > 0
                         ? SearchColumns
                         : Fields
                             .Where(f =>
                                 f.Kind == FieldKind.Text ||
                                 f.Kind == FieldKind.Multiline)
                             .Select(f => f.Name)))
            {
                parts.Add($"{col}::text ILIKE @search");
            }

            where.Add("(" + string.Join(" OR ", parts) + ")");

            parameters.Add(
                new NpgsqlParameter(
                    "search",
                    NpgsqlDbType.Text)
                {
                    Value = $"%{_txtSearch.Text.Trim()}%"
                });
        }

        if (_cmbFilterField.SelectedItem is string filterField &&
            !string.IsNullOrWhiteSpace(_txtFilterValue.Text))
        {
            where.Add($"{filterField}::text ILIKE @filter");

            parameters.Add(
                new NpgsqlParameter(
                    "filter",
                    NpgsqlDbType.Text)
                {
                    Value = $"%{_txtFilterValue.Text.Trim()}%"
                });
        }

        var sql = $"SELECT * FROM {TableName}";

        if (where.Count > 0)
        {
            sql += " WHERE " + string.Join(" AND ", where);
        }

        var sortField =
            _cmbSortField.SelectedItem as string
            ?? DefaultOrderBy;

        sql +=
            $" ORDER BY {sortField} {(_chkDesc.Checked ? "DESC" : "ASC")}";

        return sql;
    }

    protected void LoadGrid()
    {
        try
        {
            var sql =
                BuildSelectSql(out var parameters);

            var table =
                Db.GetTable(sql, parameters.ToArray());

            _binding.DataSource = table;

            _grid.DataSource = _binding;

            SetupColumnHeaders();
        }
        catch (PostgresException ex)
        {
            MessageBox.Show(
                $"Ошибка PostgreSQL:\n{ex.Message}",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void SetupColumnHeaders()
    {
        foreach (DataGridViewColumn col in _grid.Columns)
        {
            col.HeaderText = col.Name switch
            {
                "hospital_id" => "ID больницы",

                "department_id" => "ID отделения",

                "doctor_inn" => "ИНН врача",

                "patient_inn" => "ИНН пациента",

                "position_id" => "ID должности",

                "diagnosis_id" => "ID диагноза",

                "full_name" => "ФИО",

                "head_doctor" => "Заведующий",

                "hire_date" => "Дата приёма",

                "admission_date" => "Дата поступления",

                "discharge_date" => "Дата выписки",

                "diagnosis_date" => "Дата диагноза",

                "discharge_state" => "Состояние при выписке",

                "created_at" => "Дата создания",

                "treatment_method" => "Метод лечения",

                "active_patients_count" => "Активные пациенты",

                _ => col.Name
            };
        }
    }

    private void AddRecord()
    {
        using var editor =
            new RecordEditorForm(
                $"Добавить: {Text}",
                Fields);

        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                InsertRecord(editor.Values);

                LoadGrid();
            }
            catch (PostgresException ex)
            {
                MessageBox.Show(
                    $"Ошибка PostgreSQL:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    private void EditRecord()
    {
        if (_grid.CurrentRow?.DataBoundItem
            is not DataRowView drv)
            return;

        var initial =
            drv.Row.Table.Columns
                .Cast<DataColumn>()
                .ToDictionary(
                    c => c.ColumnName,
                    c => drv.Row[c.ColumnName]);

        using var editor =
            new RecordEditorForm(
                $"Изменить: {Text}",
                Fields,
                initial);

        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                UpdateRecord(editor.Values, initial);

                LoadGrid();
            }
            catch (PostgresException ex)
            {
                MessageBox.Show(
                    $"Ошибка PostgreSQL:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    private void DeleteRecord()
    {
        if (_grid.CurrentRow?.DataBoundItem
            is not DataRowView drv)
            return;

        var keyValue = drv.Row[KeyColumn];

        if (MessageBox.Show(
                "Удалить запись?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
            != DialogResult.Yes)
        {
            return;
        }

        try
        {
            Db.Execute(
                $"DELETE FROM {TableName} WHERE {KeyColumn} = @id",

                new NpgsqlParameter("id", keyValue));

            LoadGrid();
        }
        catch (PostgresException ex)
        {
            MessageBox.Show(
                $"Ошибка PostgreSQL:\n{ex.Message}",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    protected virtual void InsertRecord(
        Dictionary<string, object?> values)
    {
        var insertFields =
            Fields
                .Where(f => !f.IsIdentity)
                .ToList();

        var cols =
            string.Join(
                ", ",
                insertFields.Select(f => f.Name));

        var vals =
            string.Join(
                ", ",
                insertFields.Select(f => "@" + f.Name));

        var sql =
            $"INSERT INTO {TableName} ({cols}) VALUES ({vals})";

        var parameters =
            insertFields
                .Select(f =>
                    new NpgsqlParameter(
                        f.Name,
                        values.TryGetValue(f.Name, out var v)
                            ? (v ?? DBNull.Value)
                            : DBNull.Value))
                .ToArray();

        Db.Execute(sql, parameters);
    }

    protected virtual void UpdateRecord(
        Dictionary<string, object?> values,
        Dictionary<string, object?> initialValues)
    {
        var editableFields =
            Fields
                .Where(f =>
                    !f.IsIdentity &&
                    !f.IsPrimaryKey)
                .ToList();

        var setClause =
            string.Join(
                ", ",
                editableFields.Select(f => $"{f.Name} = @{f.Name}"));

        var sql =
            $"UPDATE {TableName} " +
            $"SET {setClause} " +
            $"WHERE {KeyColumn} = @key";

        var parameters =
            editableFields
                .Select(f =>
                    new NpgsqlParameter(
                        f.Name,
                        values.TryGetValue(f.Name, out var v)
                            ? (v ?? DBNull.Value)
                            : DBNull.Value))
                .ToList();

        parameters.Add(
            new NpgsqlParameter(
                "key",
                initialValues[KeyColumn] ?? DBNull.Value));

        Db.Execute(sql, parameters.ToArray());
    }
}
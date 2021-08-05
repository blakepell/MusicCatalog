/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Avalon.Sqlite.Common;
using Avalon.Sqlite.Editor;
using Dapper;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Data.Sqlite;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace Avalon.Sqlite
{
    /// <summary>
    /// A single instance SQLite query editor.
    /// </summary>
    public partial class SqliteQueryControl
    {
        public static readonly DependencyProperty SchemaProperty =
            DependencyProperty.Register(nameof(Schema), typeof(Schema), typeof(SqliteQueryControl), new PropertyMetadata(new Schema()));

        /// <summary>
        /// The database schema current as of the last refresh.
        /// </summary>
        public Schema Schema
        {
            get => (Schema)GetValue(SchemaProperty);
            set => SetValue(SchemaProperty, value);
        }

        public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(
            nameof(StatusText), typeof(string), typeof(SqliteQueryControl), new PropertyMetadata("Status: Idle"));

        /// <summary>
        /// The status text on the upper right hand corner of the control.
        /// </summary>
        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public static readonly DependencyProperty RefreshSchemaAfterQueryProperty = DependencyProperty.Register(
            nameof(RefreshSchemaAfterQuery), typeof(bool), typeof(SqliteQueryControl), new PropertyMetadata(true));

        /// <summary>
        /// Whether the schema should refresh after each query batch is run.
        /// </summary>
        public bool RefreshSchemaAfterQuery
        {
            get => (bool)GetValue(RefreshSchemaAfterQueryProperty);
            set => SetValue(RefreshSchemaAfterQueryProperty, value);
        }

        private DataTable _dataTable;

        /// <summary>
        /// The results of the last query.
        /// </summary>
        public DataTable DataTable
        {
            get => _dataTable;
            set
            {
                _dataTable = value;
                NotifyPropertyChanged();
            }
        }

        private string _connectionString;

        /// <summary>
        /// The connection string used to connect to the SQLite database.
        /// </summary>
        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                _connectionString = value;
                NotifyPropertyChanged();
            }
        }

        public static readonly DependencyProperty IsQueryExecutingProperty = DependencyProperty.Register(
            nameof(IsQueryExecuting), typeof(bool), typeof(SqliteQueryControl), new PropertyMetadata(false));


        /// <summary>
        /// Whether or not a query is currently executing.
        /// </summary>
        public bool IsQueryExecuting
        {
            get => (bool) GetValue(IsQueryExecutingProperty);
            set => SetValue(IsQueryExecutingProperty, value);
        }

        /// <summary>
        /// The current text in the query editor.
        /// </summary>
        public string QueryText
        {
            get => SqlEditor.Text;
            set
            {

                SqlEditor.Text = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SqliteQueryControl()
        {
            InitializeComponent();
            this.DataContext = this;

            var compact = new ResourceDictionary { Source = new Uri("/ModernWpf;component/DensityStyles/Compact.xaml", UriKind.Relative) };
            SqlResults.Resources.MergedDictionaries.Add(compact);
            TreeViewSchema.Resources.MergedDictionaries.Add(compact);
            SqlCommandBar.Resources.MergedDictionaries.Add(compact);
            DbExplorerCommandBar.Resources.MergedDictionaries.Add(compact);

            this.SetTheme();

            // Intellisense
            SqlEditor.TextArea.TextEntering += SqlEditor_TextEntering;
            SqlEditor.TextArea.TextEntered += SqlEditor_TextEntered;
        }

        /// <summary>
        /// Gets the current application theme and applies the dark or light colors to the syntax
        /// highlighting in AvalonEdit.  This defaults to the current theme of the application which
        /// will either be set by the application or it will be retrieved from the Windows default theme.
        /// </summary>
        private void SetTheme()
        {
            var theme = ThemeManager.Current.ActualApplicationTheme;
            var asm = Assembly.GetExecutingAssembly();
            string resourceName;

            switch (theme)
            {
                case ApplicationTheme.Light:
                    resourceName = $"{asm.GetName().Name}.Editor.SqliteLight.xshd";
                    break;
                case ApplicationTheme.Dark:
                    resourceName = $"{asm.GetName().Name}.Editor.SqliteDark.xshd";
                    break;
                default:
                    resourceName = $"{asm.GetName().Name}.Editor.SqliteLight.xshd";
                    break;
            }

            using (var s = asm.GetManifestResourceStream(resourceName))
            {
                if (s != null)
                {
                    using var reader = new XmlTextReader(s);
                    SqlEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        /// <summary>
        /// Sets the theme to <see cref="ElementTheme.Light"/> or <see cref="ElementTheme.Dark"/> for this
        /// control.
        /// </summary>
        /// <param name="theme"></param>
        private void SetTheme(ApplicationTheme theme)
        {
            if (theme == ApplicationTheme.Dark)
            {
                ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);
            }
            else
            {
                ThemeManager.SetRequestedTheme(this, ElementTheme.Light);
            }

            this.SetTheme();
        }

        /// <summary>
        /// Loaded event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryControl_Loaded(object sender, RoutedEventArgs e)
        {
            SqlEditor.Focus();
        }

        /// <summary>
        /// Handles code that needs to run when the control is unloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SqliteQueryControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            SqlEditor.TextArea.TextEntering -= SqlEditor_TextEntering;
            SqlEditor.TextArea.TextEntered -= SqlEditor_TextEntered;

            if (this.DataTable != null)
            {
                this.DataTable.Clear();
                this.DataTable.Dispose();
            }
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonExecuteSql_ClickAsync(object sender, RoutedEventArgs e)
        {
            // Close the auto complete window box if its open.
            _completionWindow?.Close();

            if (this.IsQueryExecuting)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(SqlEditor.Text))
            {
                this.StatusText = "0 records returned.";
                return;
            }

            this.IsQueryExecuting = true;
            this.StatusText = "Status: Executing SQL";

            // Get rid of anything in the current DataTable.
            if (this.DataTable != null)
            {
                this.DataTable.Clear();
                this.DataTable.Dispose();
                SqlResults.ItemsSource = null;
            }

            // Cross thread exception if we pass SqlText.Text into the Task, put it in string first.
            string sql = this.SqlEditor.Text;

            try
            {
                this.DataTable = await Task.Run(async () => await this.ExecuteDataTableAsync(sql));
                this.SqlResults.ItemsSource = this.DataTable.DefaultView;

                if (this.DataTable != null)
                {
                    this.StatusText = $"{DataTable?.Rows.Count.ToString().FormatIfNumber()} {"record".IfCountPluralize(DataTable?.Rows.Count ?? 0, "records")} returned.";
                }
            }
            catch (Exception ex)
            {
                this.StatusText = ex.Message;
            }

            if (this.RefreshSchemaAfterQuery)
            {
                await this.RefreshSchemaAsync();
            }

            this.IsQueryExecuting = false;
        }

        /// <summary>
        /// Refreshes the database schema so that the schema explorer shows the latest
        /// schema available in the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonRefreshSchema_ClickAsync(object sender, RoutedEventArgs e)
        {
            await this.RefreshSchemaAsync();
        }

        /// <summary>
        /// When a key is pressed anywhere on the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryControl_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                this.ButtonExecuteSql_ClickAsync(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _completionWindow?.Close();
            }
        }

        /// <summary>
        /// Event for when a column auto generates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// The underscore in column headings gets escape because of AccessKey handling.  In order for it
        /// to display correctly we need double it to prevent AccessKey handling.
        /// </remarks>
        private void SqlResults_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();

            if (!string.IsNullOrWhiteSpace(header))
            {
                e.Column.Header = header.Replace("_", "__");
            }
        }

        /// <summary>
        /// Executes a query and displays it's results on the control.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task ExecuteQueryAsync(string sql)
        {
            // Close the auto complete window box if its open.
            _completionWindow?.Close();

            if (this.IsQueryExecuting)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                this.StatusText = "0 records returned.";
                return;
            }

            this.IsQueryExecuting = true;
            this.StatusText = "Status: Executing SQL";

            // Get rid of anything in the current DataTable.
            if (this.DataTable != null)
            {
                this.DataTable.Clear();
                this.DataTable.Dispose();
                SqlResults.ItemsSource = null;
            }

            try
            {
                this.DataTable = await Task.Run(async () => await this.ExecuteDataTableAsync(sql));
                this.SqlResults.ItemsSource = this.DataTable.DefaultView;

                if (this.DataTable != null)
                {
                    this.StatusText = $"{DataTable?.Rows.Count.ToString().FormatIfNumber()} {"record".IfCountPluralize(DataTable?.Rows.Count ?? 0, "records")} returned.";
                }
            }
            catch (Exception ex)
            {
                this.StatusText = ex.Message;
            }

            if (this.RefreshSchemaAfterQuery)
            {
                await this.RefreshSchemaAsync();
            }

            this.IsQueryExecuting = false;
        }

        /// <summary>
        /// Executes SQL and returns a <see cref="DataTable"/>.
        /// </summary>
        public async Task<DataTable> ExecuteDataTableAsync(string sql)
        {
            var dt = new DataTable();

            await using (var conn = new SqliteConnection(this.ConnectionString))
            {
                await conn.OpenAsync();
                
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    await using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        // The DataSet is required to ignore constraints on the DataTable which is important
                        // because queries don't always have the constraints of the source tables (e.g. an inner joined
                        // query can bring back records with keys listed many times because of the join).
                        using (var ds = new DataSet() { EnforceConstraints = false })
                        {
                            ds.Tables.Add(dt);
                            dt.BeginLoadData();
                            dt.Load(dr);
                            dt.EndLoadData();
                            ds.Tables.Remove(dt);
                        }
                    }
                }

                await conn.CloseAsync();
            }

            return dt;
        }

        /// <summary>
        /// Refreshes the database schema.
        /// </summary>
        public async Task RefreshSchemaAsync()
        {
            var schema = new Schema();

            await using (var conn = new SqliteConnection(this.ConnectionString))
            {
                await conn.OpenAsync();

                schema.DatabaseName = Argus.IO.FileSystemUtilities.ExtractFileName(conn.DataSource);

                SqlMapper.SetTypeMap(typeof(Table), new ColumnAttributeTypeMapper<Table>());
                var ieTables = await conn.QueryAsync<Table>("select * from sqlite_master where type = 'table' order by name");
                schema.Tables = new ObservableCollection<Table>(ieTables);

                foreach (var table in schema.Tables)
                {
                    // From the official SQLite documentation: The quote() function converts its argument into a form that
                    // is appropriate for inclusion in an SQL statement.

                    // So, the PRAGMA command won't work with parameters AND selecting from pragma_table_info isn't working
                    // with this version of SQLite so we're going to use the SQLite quote function to escape our parameter
                    // before doing a string concat.  Before people claim outrage this was a recommendation from a senior
                    // development on the Microsoft Entity Framework team.
                    var dictionary = new Dictionary<string, object>
                    {
                        {"@TableName", table.TableName}
                    };

                    var parameters = new DynamicParameters(dictionary);
                    string tableName = await conn.QueryFirstAsync<string>("SELECT quote(@TableName)", parameters);

                    SqlMapper.SetTypeMap(typeof(Field), new ColumnAttributeTypeMapper<Field>());
                    var ieFields = await conn.QueryAsync<Field>($"PRAGMA table_info({tableName});", parameters);
                    table.Fields = new ObservableCollection<Field>(ieFields);
                }

                SqlMapper.SetTypeMap(typeof(View), new ColumnAttributeTypeMapper<View>());
                var ieViews = await conn.QueryAsync<View>("select * from sqlite_master where type = 'view' order by name");
                schema.Views = new ObservableCollection<View>(ieViews);

                foreach (var view in schema.Views)
                {
                    var dictionary = new Dictionary<string, object>
                    {
                        {"@ViewName", view.TableName}
                    };

                    var parameters = new DynamicParameters(dictionary);
                    string viewName = await conn.QueryFirstAsync<string>("SELECT quote(@ViewName)", parameters);

                    // See comment above about escaping via the quote SQLite function.
                    SqlMapper.SetTypeMap(typeof(Field), new ColumnAttributeTypeMapper<Field>());
                    var ieFields = await conn.QueryAsync<Field>($"PRAGMA table_info({viewName});", parameters);
                    view.Fields = new ObservableCollection<Field>(ieFields);
                }

                await conn.CloseAsync();
            }

            this.Schema = schema;
        }

        CompletionWindow _completionWindow;

        private void SqlEditor_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Text was a space.. see if the previous word was a command that has sub commands.
            if (e.Text == " ")
            {
                string word = GetWordBeforeSpace(SqlEditor);

                if (word.Equals("from", StringComparison.OrdinalIgnoreCase))
                {
                    // Open code completion after the user has pressed dot:
                    _completionWindow = new CompletionWindow(SqlEditor.TextArea);
                    var data = _completionWindow.CompletionList.CompletionData;

                    // Add tables
                    foreach (var item in Schema.Tables)
                    {
                        data.Add(new CompletionData(item.TableName));
                    }

                    // Add views
                    foreach (var view in Schema.Views)
                    {
                        data.Add(new CompletionData(view.TableName));
                    }

                    _completionWindow.Show();
                    _completionWindow.Closed += delegate
                    {
                        _completionWindow = null;
                    };
                }
                else
                {
                    return;
                }
            }

            if (e.Text == ".")
            {
                // Open code completion after the user has pressed dot:
                _completionWindow = new CompletionWindow(SqlEditor.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;

                foreach (var table in Schema.Tables)
                {
                    foreach (var field in table.Fields)
                    {
                        data.Add(new CompletionData(field.Name, $"Type: {field.Type}\r\nNot Null: {field.NotNull.ToString()}\r\nDefault Value: {field.DefaultValue}"));
                    }
                }

                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }
        }

        public static string GetWordBeforeSpace(TextEditor textEditor)
        {
            var wordBeforeDot = string.Empty;
            var caretPosition = textEditor.CaretOffset - 2;
            var lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(caretPosition));
            string text = textEditor.Document.GetText(lineOffset, 1);

            while (true)
            {
                if (text == null && string.Compare(text, " ", StringComparison.Ordinal) > 0)
                {
                    break;
                }

                if (Regex.IsMatch(text, @".*[^A-Za-z\. ]"))
                {
                    break;
                }

                if (text != " ")
                {
                    wordBeforeDot = text + wordBeforeDot;
                }

                if (caretPosition == 0)
                {
                    break;
                }

                lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(--caretPosition));

                text = textEditor.Document.GetText(lineOffset, 1);
            }

            return wordBeforeDot;
        }

        private void SqlEditor_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void TreeViewMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as MenuItem;
            string cmd;
            string tag;
            int counter = 0;

            if (item == null)
            {
                return;
            }

            StringBuilder sb;
            Table table;
            cmd = item.CommandParameter.ToString();
            tag = item.Tag.ToString();

            switch (cmd)
            {
                case "SelectAll":
                    await this.ExecuteQueryAsync($"select * from {tag}");
                    break;
                case "Select1000":
                    await this.ExecuteQueryAsync($"select * from {tag} limit 1000");
                    break;
                case "GenerateSelect":
                    table = this.Schema.Tables.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (table == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    sb = new StringBuilder();

                    if (this.SqlEditor.Text.Length > 0)
                    {
                        sb.Append("\r\n\r\n");
                    }

                    sb.Append("SELECT ");

                    foreach (var field in table.Fields)
                    {
                        counter++;

                        if (counter > 1)
                        {
                            sb.AppendFormat("\r\n    , [{0}] -- {1}", field.Name, field.Type);
                        }
                        else
                        {
                            sb.AppendFormat("\r\n      [{0}] -- {1}", field.Name, field.Type);
                        }

                        if (field.PrimaryKey)
                        {
                            sb.Append(", PK");
                        }
                    }

                    sb.TrimEnd(',');
                    sb.AppendFormat("\r\nFROM [{0}];", table.Name);

                    SqlEditor.AppendText(sb.ToString());

                    break;
                case "GenerateInsert":
                    table = this.Schema.Tables.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (table == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    sb = new StringBuilder();

                    if (this.SqlEditor.Text.Length > 0)
                    {
                        sb.Append("\r\n\r\n");
                    }
                    
                    sb.AppendFormat("INSERT INTO [{0}] (", table.Name);

                    foreach (var field in table.Fields)
                    {
                        counter++;

                        if (counter > 1)
                        {
                            sb.AppendFormat("\r\n    , [{0}]", field.Name);
                        }
                        else
                        {
                            sb.AppendFormat("\r\n      [{0}]", field.Name);
                        }
                    }

                    counter = 0;
                    sb.Append("\r\n) VALUES (");

                    foreach (var field in table.Fields)
                    {
                        counter++;

                        if (counter > 1)
                        {
                            sb.AppendFormat("\r\n    , @{0} -- {1}", field.Name, field.Type);
                        }
                        else
                        {
                            sb.AppendFormat("\r\n      @{0} -- {1}", field.Name, field.Type);
                        }

                        if (field.PrimaryKey)
                        {
                            sb.Append(", PK");
                        }
                    }

                    sb.Append("\r\n);");

                    SqlEditor.AppendText(sb.ToString());

                    break;
                case "GenerateUpdate":
                    table = this.Schema.Tables.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (table == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    sb = new StringBuilder();

                    if (this.SqlEditor.Text.Length > 0)
                    {
                        sb.Append("\r\n\r\n");
                    }

                    sb.AppendLine("-- Note: Remember to put a WHERE statement on this if you need it.");
                    sb.AppendFormat("UPDATE [{0}] SET ", table.Name);

                    foreach (var field in table.Fields)
                    {
                        counter++;

                        if (counter > 1)
                        {
                            sb.AppendFormat("\r\n     , [{0}] = @{0} -- {1}", field.Name, field.Type);
                        }
                        else
                        {
                            sb.AppendFormat("\r\n       [{0}] = @{0} -- {1}", field.Name, field.Type);
                        }

                        if (field.PrimaryKey)
                        {
                            sb.Append(", PK");
                        }
                    }

                    sb.Append("\r\n;");

                    SqlEditor.AppendText(sb.ToString());

                    break;
                case "CreateTable":
                    table = this.Schema.Tables.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (table == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    if (this.SqlEditor.Text.Length > 0)
                    {
                        this.SqlEditor.AppendText("\r\n\r\n");
                    }

                    this.SqlEditor.AppendText(table.Sql);

                    break;
                case "CreateView":
                    var view = this.Schema.Views.FirstOrDefault(x => x.TableName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                    if (view == null)
                    {
                        this.StatusText = $"Error: Table '{tag}' not found.";
                        return;
                    }

                    if (this.SqlEditor.Text.Length > 0)
                    {
                        this.SqlEditor.AppendText("\r\n\r\n");
                    }

                    this.SqlEditor.AppendText(view.Sql);

                    break;

            }
        }
    }
}
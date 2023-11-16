using System;
using System.Data; // Operaciones con bases de datos
using System.Data.SQLite; // Acceso a la Base de Datos SQLite
using System.Drawing; // Manipulación de imágenes y gráficos
using System.IO; // Operaciones con archivos y directorios
using System.Linq; // Extensiones LINQ para consultas
using System.Media; // Reproducción de sonidos
using System.Text.RegularExpressions; // Expresiones regulares para validación
using System.Windows.Forms; // Elementos gráficos de Windows Forms

namespace Final_final_de_verdad
{
    public partial class dada : Form
    {
        public dada()
        {
            InitializeComponent(); // Inicia form
            string connectionString = "Data Source=BBDDFINAL.db;Version=3;"; // Creación de la base de datos
            using (SQLiteConnection connection = new SQLiteConnection(connectionString)) // Variable para establecer la conexión desde SQLite
            {
                connection.Open(); // Inicia SQLite
                string createTableQuery = "CREATE TABLE IF NOT EXISTS ALUMNOS (NumRegistro INTEGER PRIMARY KEY, Nombre TEXT, Apellido TEXT, " +
                    "Matricula INTEGER, Edad INTEGER, Email TEXT, Carrera TEXT)"; // Se establecen las comlumnas de la BBDD en caso de que no exista
                using (SQLiteCommand createTableCommand = new SQLiteCommand(createTableQuery, connection)) // Variable para llamar la creación de la columna
                {
                    createTableCommand.ExecuteNonQuery();
                }
            }
            using (SQLiteConnection connection = new SQLiteConnection(connectionString)) // Inicio bd y vuelvo al DataGridView
            {
                connection.Open();
                string selectQuery = "SELECT * FROM ALUMNOS";// Cada vez que se abre el form se selecciona la tabla

                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command)) // Variable para adaptar al DataGridView
                    {
                        DataTable tablaAlumnos = new DataTable();
                        adapter.Fill(tablaAlumnos);
                        dgvTabla.DataSource = tablaAlumnos; // Se agregan los campos y los registros existentes de la DDBB al DGV
                    }
                }
            }
            //-----------------------------Detalles--------------------------------------------------------------
            {
                //-----------------Configuración de colores para el DataGridView----------------------------------------
                dgvTabla.DefaultCellStyle.BackColor = Color.LightSkyBlue; // Color de fondo de las celdas
                dgvTabla.DefaultCellStyle.ForeColor = Color.Black;    // Color de texto de las celdas
                dgvTabla.BackgroundColor = Color.Black;          // Color de fondo del DataGridView
                dgvTabla.RowHeadersDefaultCellStyle.BackColor = Color.Black;  // Color de fondo de las cabeceras de fila
                dgvTabla.ColumnHeadersDefaultCellStyle.BackColor = Color.Black; // Color de fondo de las cabeceras de columna
                dgvTabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black; // Color de texto de las cabeceras de columna
                //------------------------------------------------------------------------------------------------------
                //-----------------Habilitación de función para botón "Ayuda"-------------------------------------------
                this.HelpButton = true;
                this.HelpButtonClicked += dada_HelpButtonClicked;
                //------------------------------------------------------------------------------------------------------
                //-----------------Recurso al editar el DGV que no respeten los campos----------------------------------
                dgvTabla.DataError += dataGridView1_DataError;
                //------------------------------------------------------------------------------------------------------
                //-----------------Recurso para cerrar la app-----------------------------------------------------------
                this.FormClosing += dada_FormClosing;
                //------------------------------------------------------------------------------------------------------
                //---------------Acomoda el form------------------------------------------------------------------------
                this.Resize += dada_Resize;
                //------------------------------------------------------------------------------------------------------
                //----------Bloquea columna Carreras--------------------------------------------------------------------      
                dgvTabla.CellBeginEdit += dgvTabla_CellBeginEdit;
                //------------------------------------------------------------------------------------------------------
                //----------Enter a la fila para facilitar el edit------------------------------------------------------
                dgvTabla.CellEnter += dgvTabla_CellEnter;
                //------------------------------------------------------------------------------------------------------
                //----------Bienvenida----------------------------------------------------------------------------------
                Font messageBoxFont = new Font("Arial", 12, FontStyle.Bold);
                string mensajeBienvenida =
                    "Esta aplicación está diseñada para recopilar información tanto de carácter académico como personal de posibles alumnos de un instituto. Cada registro se presenta de manera visual para mejorar la adaptabilidad del programa, facilitando así el acceso a la información y la exportación de datos estructurados. La gestión del programa se ha optimizado para garantizar su correcto funcionamiento. \n\n" +
                    "La interfaz se divide en tres secciones:\n" +
                    "- La parte superior permite ingresar nuevos datos,\n" +
                    "- La sección intermedia facilita la manipulación de datos ingresados y registrados, y\n" +
                    "- La parte inferior muestra una lista de registros con información detallada.\n\n" +
                    "Además, hay una sección operativa dedicada a realizar actualizaciones.\n\n" +
                    "¿Deseas continuar utilizando la aplicación?";
                /*Muestra el msj y evalua la elección*/
                if (MessageBox.Show(mensajeBienvenida, "Información", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
                {
                    // Si el usuario hace clic en "Cancelar", abre form para pasar al cierre del formulario
                    cerrarFormulario = true;
                }
                //------------------------------------------------------------------------------------------------------
                //-----------------Mas detalles-------------------------------------------------------------------------
                soundPlayer = new SoundPlayer("C:\\Users\\JCAISIA\\Music\\cellthing.wav");
                dgvTabla.CellClick += dgvTabla_CellClick;
                dgvTabla.KeyDown += dgvTabla_KeyDown;
                //------------------------------------------------------------------------------------------------------
            }
            //---------------------------------------------------------------------------------------------------
        }
        //------------------------------Variables de uso--------------------------------------------------------//
        private SoundPlayer soundPlayer; /* Variable para el reproductor de sonido*/
        private bool cerrarFormulario = false; /* Variable para cerrar formulario*/
        private bool chgeEstado = false; /*Variable para cambiar el estado del DGV y largar mensaje*/
        //------------------------------------------------------------------------------------------------------//
        //------------------------------Botones funcionales-----------------------------------------------------//
        private void btnCargar_Click(object sender, EventArgs e)
        {
            string connectionString = "Data Source=BBDDFINAL.db;Version=3;";
            //-------------Variables de uso para los txb------------------------------------------------------------
            string pattern = @"^[a-zA-Z0-9_.+-]+@(hotmail\.com|hotmail\.com\.ar|gmail\.com|outlook\.com)$";
            string nombre = txbNombre.Text;
            string apellido = txbApellido.Text;
            int matricula;
            int edad;
            string email = txbEmail.Text;
            string carrera = cbCarrera.Text;
            //------------------------------------------------------------------------------------------------------
            //------------Condiciones de validaciones al apretar el botón-------------------------------------------
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("Ingrese todos los datos antes de cargar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return; // No continuar con la carga de datos
                }
                if (!ValidarNombre(nombre)) /*Validación de nombre*/
                {
                    MessageBox.Show("Ingresar nombre válido.", "Error de lectura", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show("El nombre NO debe contener números o caracteres especiales.", "Consejo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (!ValidarApellido(apellido)) /*Validación de apellido*/
                {
                    MessageBox.Show("Ingresar apellido válido.", "Fatal error de lectura", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show("El apellido NO debe contener números, caracteres especiales o un genio al teclado.", "Consejo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!int.TryParse(txbMatricula.Text, out matricula)) /*Validación de DNI*/
                {
                    MessageBox.Show("Matricula/DNI debe ser un número entero válido.", "2 + 2 = 5?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show("Para registrar nueva matricula con DNI se requiere utilizar números enteros del 0 al 9", "Consejo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    if (txbMatricula.Text.Length != 8)/*Obliga a usar correctamente los 8 números del DNI o llenar con 0*/
                    {
                        MessageBox.Show("Ingresar valor DNI de 8 números, en caso de tener menos números, llenar con 0. Por ejemplo: 00123456.", "No podes equivocarte tanto...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                if (!int.TryParse(txbEdad.Text, out edad)) /*Validación para saber si es un valor entero*/
                {
                    MessageBox.Show("Edad debe ser un número entero válido.", "Error 404 Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    int valEdad = edad;
                    if (!(valEdad > 16 && valEdad < 51)) /*Validación para el rango dentro de los 17 a 51 años de edad*/
                    {
                        MessageBox.Show("Ingresar edad entre 17 a 50 años.", "Edad denegada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                if (!Regex.IsMatch(email, pattern)) /*Validación de email*/
                {
                    MessageBox.Show("Ingrese un correo valido: @hotmail, @gmail...", "Email no permitido", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show("Es mas viable molestar al usuario con múltiples ventanas hasta que aprenda a usar el programa?", "Sabías qué...", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return;
                }
                if (string.IsNullOrWhiteSpace(carrera)) /*Obliga a elegir una carrera si no se selecciona o esta vacío el campo*/
                {
                    MessageBox.Show("Por favor, seleccione una carrera.", "Te olvidaste lo importante", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return;
                }
                string datosAMostrar = $"Nombre: {nombre}\n" +
                           $"Apellido: {apellido}\n" +
                           $"Matrícula: {matricula}\n" +
                           $"Edad: {edad}\n" +
                           $"Email: {email}\n" +
                           $"Carrera: {carrera}";
                DialogResult result = MessageBox.Show($"¿Estás seguro de cargar los siguientes datos?\n\n{datosAMostrar}", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))/*Llamar a la conexión*/
                    {
                        connection.Open();
                        string insertQuery = "INSERT INTO ALUMNOS (nombre, apellido, Matricula, edad, email, carrera) VALUES (@nombre, @apellido, @Matricula, @edad, @email, @carrera)"; /*Consulta de carga SQL*/
                        using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                        {
                            //------Actualización de valores-------------------------------------------------
                            insertCommand.Parameters.AddWithValue("@nombre", nombre);
                            insertCommand.Parameters.AddWithValue("@apellido", apellido);
                            insertCommand.Parameters.AddWithValue("@Matricula", matricula);
                            insertCommand.Parameters.AddWithValue("@edad", edad);
                            insertCommand.Parameters.AddWithValue("@email", email);
                            insertCommand.Parameters.AddWithValue("@carrera", carrera);
                            insertCommand.ExecuteNonQuery();
                        }
                        DiosQueAnde();
                    }
                    //----------Limpia los campos luego de usarse--------------------------------------------
                    emptyIn();
                    //---------------------------------------------------------------------------------------
                    MessageBox.Show("Los datos fueron ingresados a nuestra plataforma.", "Registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Registro anulado", "Operación finalizada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (SQLiteException ex)
            {
                //-----Excepciones específicas de SQLite-------------------------------------------------
                MessageBox.Show("Error de SQLite: " + ex.Message, "Error al registrar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                //------Otras excepciones--------------------------------------------------------
                MessageBox.Show("Error general: " + ex.Message, "Error al registrar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnEditar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvTabla.SelectedRows.Count > 0)
                {
                    int rowIndex = dgvTabla.SelectedCells[0].RowIndex;
                    DataGridViewRow selectedRow = dgvTabla.Rows[rowIndex];

                    // Verificar si alguna celda está vacía
                    if (selectedRow.Cells.Cast<DataGridViewCell>().Any(cell => string.IsNullOrEmpty(Convert.ToString(cell.Value))))
                    {
                        MessageBox.Show("No se puede editar porque hay celdas vacías.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    string numreg = selectedRow.Cells["NumRegistro"].Value.ToString();
                    string nombre = selectedRow.Cells["Nombre"].Value.ToString();
                    string apellido = selectedRow.Cells["Apellido"].Value.ToString();
                    string matricula = selectedRow.Cells["Matricula"].Value.ToString();
                    string edad = selectedRow.Cells["Edad"].Value.ToString();
                    string email = selectedRow.Cells["Email"].Value.ToString();
                    string carrera = selectedRow.Cells["Carrera"].Value.ToString();
                    if (SoloLetras(nombre) && SoloLetras(apellido))
                    {
                        string mensaje = $"Estás por editar la siguiente fila:\n\n" +
                             $"Nombre: {nombre}\n" +
                             $"Apellido: {apellido}\n" +
                             $"Matrícula: {matricula}\n" +
                             $"Edad: {edad}\n" +
                             $"Email: {email}\n" +
                             $"Carrera: {carrera}\n\n" +
                             "¿Estás seguro de continuar?";

                        DialogResult result = MessageBox.Show(mensaje, "Confirmar edición", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        //DialogResult result = MessageBox.Show("¿Quieres editar la fila seleccionada?", "Confirmar edición", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            string connectionString = "Data Source=BBDDFINAL.db;Version=3;";
                            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                            {
                                connection.Open();
                                string updateQuery = "UPDATE ALUMNOS SET NumRegistro = @numregistro, Nombre = @nombre, Apellido = @apellido, Matricula = @matricula, Edad = @edad, Email = @email, Carrera = @carrera WHERE NumRegistro = @numregistro";
                                using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                                {
                                    command.Parameters.AddWithValue("@numregistro", numreg);
                                    command.Parameters.AddWithValue("@nombre", nombre);
                                    command.Parameters.AddWithValue("@apellido", apellido);
                                    command.Parameters.AddWithValue("@matricula", matricula);
                                    command.Parameters.AddWithValue("@edad", edad);
                                    command.Parameters.AddWithValue("@email", email);
                                    command.Parameters.AddWithValue("@carrera", carrera);
                                    command.ExecuteNonQuery();
                                }
                            }
                            MessageBox.Show("Datos editados.", "Enhorabuena", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ingrese un dato válido para Nombre y Apellido. Solo letras.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Seleccione una fila para editar.", "Error de lectura", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al editar: " + ex.Message, "Se detectaron problemas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            DiosQueAnde();
        }
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvTabla.SelectedRows.Count == 0)
                {
                    MessageBox.Show("No ha seleccionado fila para borrar.", "Error de lectura", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int selIndex = dgvTabla.SelectedRows[0].Index;
                string delNomb = dgvTabla.Rows[selIndex].Cells["Nombre"].Value.ToString();
                string delApe = dgvTabla.Rows[selIndex].Cells["Apellido"].Value.ToString();
                string delMatricula = dgvTabla.Rows[selIndex].Cells["Matricula"].Value.ToString();
                string delEdad = dgvTabla.Rows[selIndex].Cells["Edad"].Value.ToString();
                string delEmail = dgvTabla.Rows[selIndex].Cells["Email"].Value.ToString();
                string delCarrera = dgvTabla.Rows[selIndex].Cells["Carrera"].Value.ToString();
                // Mostrar todos los datos antes de borrar
                DialogResult result = MessageBox.Show($"Estás por borrar el siguiente registro:\n\n" +
                                                      $"Nombre: {delNomb}\n" +
                                                      $"Apellido: {delApe}\n" +
                                                      $"Matrícula: {delMatricula}\n" +
                                                      $"Edad: {delEdad}\n" +
                                                      $"Email: {delEmail}\n" +
                                                      $"Carrera: {delCarrera}\n\n" +
                                                      "¿Estás seguro de continuar?", "Alerta - Irrecuperable", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel)
                {
                    return;
                }
                try
                {
                    string numregistro = dgvTabla.Rows[selIndex].Cells["NumRegistro"].Value.ToString();
                    string connectionString = "Data Source=BBDDFINAL.db;Version=3;";

                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        // Eliminar la fila seleccionada
                        string deleteQuery = "DELETE FROM ALUMNOS WHERE NumRegistro = @NumRegistro";
                        using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@NumRegistro", numregistro);
                            command.ExecuteNonQuery();
                        }
                        // Actualizar los números de registro en la base de datos
                        string updateQuery = "UPDATE ALUMNOS SET NumRegistro = NumRegistro - 1 WHERE NumRegistro > @NumRegistro";
                        using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@NumRegistro", numregistro);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                    DiosQueAnde();
                    MessageBox.Show("Datos eliminados satisfactoriamente", "Ejecución finalizada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al borrar datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al borrar datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                    saveFileDialog.Title = "Guardar archivo de exportación";
                    saveFileDialog.DefaultExt = "txt";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string connectionString = "Data Source=BBDDFINAL.db;Version=3;";

                        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                        {
                            connection.Open();

                            string selectQuery = "SELECT * FROM ALUMNOS";

                            using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                            {
                                using (SQLiteDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        // Crear un archivo de texto en la ubicación seleccionada
                                        using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                                        {
                                            // Escribir encabezados
                                            WriteHeaders(writer, reader);

                                            // Escribir datos
                                            WriteData(writer, reader);

                                            MessageBox.Show("Datos exportados exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("No hay datos para exportar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string busqueda = txbBuscar.Text.Trim();
            string selectQuery = "SELECT * FROM ALUMNOS WHERE Nombre LIKE @parametro OR Apellido LIKE @parametro OR Matricula LIKE @parametro OR Edad LIKE @parametro";/*Consulta de búsqueda SQL*/
            string connectionString = "Data Source=BBDDFINAL.db;Version=3;";
            try
            {
                if (string.IsNullOrEmpty(busqueda))
                {
                    MessageBox.Show("Ingrese un parámetro para continuar con la búsqueda.", "Error de lectura", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return; // No continuar con la búsqueda de datos               
                }
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@parametro", "%" + busqueda + "%");

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dgvTabla.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar datos: {ex.Message}", "Error de búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                emptyIn();
            }
        }
        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            string carreraFiltrar = cbBusCarrera.Text.Trim();
            try
            {
                if (string.IsNullOrEmpty(carreraFiltrar))
                {
                    MessageBox.Show("Ingrese un parámetro para continuar con la búsqueda.", "Error de lectura", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                string selectQuery = "SELECT * FROM ALUMNOS WHERE Carrera LIKE @carrera";
                string connectionString = "Data Source=BBDDFINAL.db;Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@carrera", $"%{carreraFiltrar}%");

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dgvTabla.DataSource = dataTable;
                        }
                    }
                }
                emptyIn();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar datos: {ex.Message}", "Error de filtrado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Estás por refrescar el registro. Si realizaste cambios, se perderán. ¿Quieres continuar?", "Alerta Base de Datos", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                // Llama la función para actualizar el DGV a la última versión
                DiosQueAnde();
                MessageBox.Show("Tabla actualizada", "Actualización Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        //------------------------------------------------------------------------------------------------------//
        //------------------------------Detalles funcionales en DGV---------------------------------------------//
        private void dgvTabla_KeyDown(object sender, KeyEventArgs e)
        {
            // Verifica si una tecla de flecha fue presionada
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                // Reproduce el sonido cuando te desplazas por las celdas con las flechas
                if (soundPlayer != null)
                {
                    soundPlayer.Play();
                }
            }
            // Verifica si la tecla Tab fue presionada
            else if (e.KeyCode == Keys.Tab)
            {
                // Reproduce el sonido cuando se presiona la tecla Tab
                if (soundPlayer != null)
                {
                    soundPlayer.Play();
                }
            }
        }
        private void dgvTabla_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (soundPlayer != null)
            {
                soundPlayer.Play();
            }
        }
        private void dgvTabla_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void dgvTabla_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            string columnaActual = dgvTabla.Columns[e.ColumnIndex].Name;

            if (columnaActual == "Carrera" || columnaActual == "Email" || columnaActual == "NumRegistro")
            {
                e.Cancel = true;
                string mensaje = $"Para editar {columnaActual.ToLower()}, es necesario reingresar el alumno. Elimine el registro y cárguelo nuevamente.";
                MessageBox.Show(mensaje, "Columna protegida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dgvTabla_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            dgvTabla.Rows[e.RowIndex].Selected = true;
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!chgeEstado)
            {
                MessageBox.Show("Alterar un registro puede perjudicar la estructura de la tabla, dejar una celda 'vacía' puede afectar el funcionamiento del sistema.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                chgeEstado = true;
            }
        }
        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
            MessageBox.Show("Error al editar: verifique los campos y compruebe los datos ingresados para continuar. Pulse ESC para salir del error.", "Error de lectura", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        //------------------------------------------------------------------------------------------------------//
        //------------------------------Detalles funcionales en FORM--------------------------------------------//
        private void dada_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("¿Seguro que quieres cerrar la aplicación? Si no ha guardado los registros se perderán", "Confirmar cierre", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true; // Cancela el cierre del formulario si el usuario elige "No"
            }
        }
        private void dada_Resize(object sender, EventArgs e)
        {
            dgvTabla.Width = this.ClientSize.Width - dgvTabla.Left * 2;
            dgvTabla.Height = this.ClientSize.Height - dgvTabla.Top * 2;
        }
        private void dada_HelpButtonClicked(object sender, EventArgs e)
        {
            string mensaje = "1- Los campos de datos están diseñados para ingresar datos permitidos\n" +
                             "2- Para editar o eliminar es necesario seleccionar la fila\n" +
                             "3- Un registro a una carrera es único, para cambiar de carrera es necesario reingresar los datos, previamente eliminando el registro original";
            MessageBox.Show(mensaje, "Ayuda", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        //------------------------------------------------------------------------------------------------------//
        //------------------------------Metodos para validación de datos----------------------------------------//
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Verifica si se debe cerrar el formulario después de mostrarse completamente
            if (cerrarFormulario)
            {
                this.Close();
            }
        }
        static bool ValidarNombre(string nombre)
        {
            //---------Patrón que permite solo letras y espacios----------------------------------------------------
            string pattNom = "^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ ]+$";/*Variable*/
            //---------Verificar si el nombre coincide con el patrón------------------------------------------------
            return Regex.IsMatch(nombre, pattNom);
        }
        static bool ValidarApellido(string apellido)
        {
            //---------Patrón que permite solo letras y espacios----------------------------------------------------
            string patApe = "^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ ]+$";/*Variable*/
            //---------Verificar si el apellido coincide con el patrón----------------------------------------------
            return Regex.IsMatch(apellido, patApe);
        }
        private void emptyIn()
        {
            txbNombre.Text = "";
            txbApellido.Text = "";
            txbMatricula.Text = "";
            txbEdad.Text = "";
            txbEmail.Text = "";
            txbBuscar.Text = "";
            cbCarrera.SelectedIndex = -1;
            cbBusCarrera.SelectedIndex = -1;
        }
        private void DiosQueAnde()
        {
            string connectionString = "Data Source=BBDDFINAL.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM ALUMNOS";
                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                    {
                        DataTable tablaAlumnos = new DataTable();
                        adapter.Fill(tablaAlumnos);
                        dgvTabla.DataSource = tablaAlumnos;
                    }
                }
            }
        }
        private bool SoloLetras(string texto)
        {
            return texto.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }
        //------------------------------------------------------------------------------------------------------//
        //------------------------------Metodos para la exportación en .TXT-------------------------------------//
        private void WriteHeaders(StreamWriter writer, SQLiteDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                writer.Write(reader.GetName(i));
                if (i < reader.FieldCount - 1)
                {
                    writer.Write("\t");
                }
            }
            writer.WriteLine();
        }
        private void WriteData(StreamWriter writer, SQLiteDataReader reader)
        {
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    writer.Write(reader[i]);
                    if (i < reader.FieldCount - 1)
                    {
                        writer.Write("\t");
                    }
                }
                writer.WriteLine();
            }
        }
        //------------------------------------------------------------------------------------------------------//
        /*------------------------------------------------------------------------------------------------------*/
        /*------------------------------------------Espacios vacíos---------------------------------------------*/
        /*------------------------------------------------------------------------------------------------------*/
        private void label1_Click(object sender, EventArgs e)
        {
        }/*----------------------*/
        /*-------------------*/
        private void label3_Click(object sender, EventArgs e)
        {
        }/*----------------------*/
        /*-------------------*/
        private void txbMatricula_TextChanged(object sender, EventArgs e)
        {
        }/*----------*/
        /*-------------------*/
        private void cbBusCarrera_SelectedIndexChanged(object sender, EventArgs e)
        {
        }/*-*/
        /*-------------------*/
        private void label7_Click(object sender, EventArgs e)
        {
        }/*----------------------*/
        /*-------------------*/
        private void label8_Click(object sender, EventArgs e)
        {
        }/*----------------------*/
        /*-------------------*/
        private void txbNombre_TextChanged(object sender, EventArgs e)
        {
        }/*-------------*/
        /*-------------------*/
        private void txbApellido_TextChanged(object sender, EventArgs e)
        {
        }/*-----------*/
        /*-------------------*/
        private void txbEdad_TextChanged(object sender, EventArgs e)
        {
        }/*---------------*/
        /*-------------------*/
        private void cbCarrera_SelectedIndexChanged(object sender, EventArgs e)
        {
        }/*----*/
        /*-------------------*/
        private void Form1_Load(object sender, EventArgs e)
        {
        }/*------------------------*/
        /*------------------------------------------------------------------------------------------------------*/
        /*------------------------------------------------------------------------------------------------------*/
        /*------------------------------------------------------------------------------------------------------*/
    }
}
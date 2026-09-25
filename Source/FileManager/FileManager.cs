using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace GSM.Core.Source.FileManager
{
    public class FileManager
    {
        // Core Properties
        // Device State
        // Internal Core Helpers
        
        //Constant
        private const int _fileCount = 1000;
        //End : Constant

        //Enums
        //Status
        public enum STATUS
        {
            INVALID = 0,
            READY = 1
        }
        //End : Status
        //Error Code
        public enum ERROR_CODE
        {
            NO_ERROR = 0,
            CONSTRUCTOR_FAIL = 1,
            CREATE_FILE = 2,
            DELETE_ALL = 3,
            DELETE_FILE = 4,
            EXIST_FILE = 5
        }
        //End : Error Code
        //Enums

        //Variables
        private string _fileName;
        private string _lastFileName;
        private string _fileType;
        private string _filePath;
        private STATUS _status;
        private ERROR_CODE _errorCode;
        private string _errorMessage;
        //End : Variables

        //Getter & Setter
        public STATUS Status
        {
            get => this._status;
        }
        public ERROR_CODE ErrorCode
        {
            get => this._errorCode;
        }
        public string ErrorMessage
        {
            get => this._errorMessage;
        }
        public string MainFile
        {
            get => this._fileName + this._fileType;
        }
        public string LastCreateFile
        {
            get => this._lastFileName + this._fileType;
        }
        //Getter & Setter

        //Constructor & DeConstructor
        public FileManager(string fileName = "DllManshoorGSM")
        {
            try
            {
                this.ResetError();
                if (fileName.Length <= 0)
                {
                    this._fileName = "DllManshoorGSM";
                }
                else
                {
                    this._fileName = fileName;
                }
                this._fileType = ".dll";
                this._filePath = AppDomain.CurrentDomain.BaseDirectory;
                this._status = STATUS.READY;
                this._lastFileName = string.Empty;
                if (!this.CheckFileExist(this._filePath, this._fileName, this._fileType))
                {
                    FileManagerFail(this._filePath + this._fileName + this._fileType + " NOT Exist");
                }
                else
                {
                    this.DeleteAll();
                }
            }
            catch (Exception ex)
            {
                FileManagerFail(ex.Message);
            }
        }
        public FileManager()
        {
            try
            {
                this.ResetError();
                this._fileName = "DllManshoorGSM";
                this._fileType = ".dll";
                this._filePath = AppDomain.CurrentDomain.BaseDirectory;
                this._status = STATUS.READY;
                this._lastFileName = string.Empty;
                if (!this.CheckFileExist(this._filePath , this._fileName , this._fileType))
                {
                    FileManagerFail(this._filePath + this._fileName + this._fileType + " NOT Exist");
                }
                else
                { 
                    this.DeleteAll(); 
                }
            }
            catch (Exception ex)
            {
                FileManagerFail(ex.Message);
            }
        }
        private void FileManagerFail(string ErrorMessage)
        {
            this._fileName = "";
            this._fileType = "";
            this._filePath = "";
            this._lastFileName = string.Empty;
            this._status = STATUS.INVALID;
            this._errorCode = ERROR_CODE.CONSTRUCTOR_FAIL;
            this._errorMessage = ErrorMessage;
        }
        
        ~FileManager()
        {
            this.DeleteAll();
        }

        private bool CheckFileExist(string FilePath, string FileName, string FileType)
        {
            try
            {
                string fileName;
                fileName = Path.Combine(FilePath, FileName + FileType);
                return System.IO.File.Exists(fileName);
            }
            catch (Exception)
            {
                this._errorCode = ERROR_CODE.EXIST_FILE;
                return false;
            }
        }
        //End : Constructor & DeConstructor

        //Create New File
        public bool CreateNewFile(out string FileName)
        {
            try
            {
                bool result = true;
                FileName = string.Empty;
                this.ResetError();
                
                if (result)
                {
                    result = this.CreateNewFileStatusCheck();
                }

                if (result)
                {
                    result = this.CreateNewFileProcess(out FileName);
                }

                if (result)
                {
                    this._lastFileName = FileName;
                }
                else
                {
                    this._lastFileName = string.Empty;
                }

                return result;
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.CREATE_FILE;
                this._errorMessage = ex.Message;
                FileName = string.Empty;
                return false;
            }
        }
        private bool CreateNewFileStatusCheck()
        {
            if (this._status == STATUS.INVALID)
            {
                this._errorCode = ERROR_CODE.CREATE_FILE;
                this._errorMessage = "CreateNewFileStatusCheck Error";
                return false;
            }
            else
            { 
                return true; 
            }
        }
        private bool CreateNewFileProcess(out string FileName)
        {
            try
            {
                FileName = string.Empty;
                bool fileFind = true;
                int fileNumber = 0;
                string sourceFile, destFile;
                sourceFile = Path.Combine(this._filePath, this._fileName + this._fileType);
                while (fileFind && fileNumber < _fileCount)
                {
                    destFile = Path.Combine(this._filePath, this._fileName + fileNumber.ToString() + this._fileType);
                    fileFind = System.IO.File.Exists(destFile);
                    if (!fileFind)
                    {
                        System.IO.File.Copy(sourceFile, destFile, true);
                        FileName = this._fileName + fileNumber.ToString() + this._fileType;
                    }
                    fileNumber++;
                }
                return !fileFind;
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.CREATE_FILE;
                this._errorMessage = ex.Message;
                FileName = string.Empty;
                return false;
            }
        }
        //End : Create New File

        //Delete All
        public bool DeleteAll()
        {
            try
            {
                bool result = true;
                this.ResetError();

                if (result)
                {
                    result = this.DeleteAllStatusCheck();
                }

                if (result)
                {
                    result = this.DeleteAllProcess();
                }

                return result;
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DELETE_ALL;
                this._errorMessage = ex.Message;
                return false;
            }
        }
        private bool DeleteAllStatusCheck()
        {
            if (this._status == STATUS.INVALID)
            {
                this._errorCode = ERROR_CODE.DELETE_ALL;
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool DeleteAllProcess()
        {
            try
            {
                string[] files;
                string mainFile;
                files = Directory.GetFiles(this._filePath, this._fileName + "*" + this._fileType);
                mainFile = Path.Combine(this._filePath, this._fileName + this._fileType);
                foreach (string file in files)
                {
                    if (file != mainFile)
                    {
                        System.IO.File.Delete(file);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DELETE_ALL;
                this._errorMessage = ex.Message;
                return false;
            }
        }
        //End : Delete All

        //Delete All
        public bool DeleteFile(string FileName)
        {
            try
            {
                bool result = true;
                this.ResetError();

                if (result)
                {
                    result = this.DeleteFileStatusCheck();
                }

                if (result)
                {
                    result = this.DeleteFileProcess(FileName);
                }

                return result;
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DELETE_FILE;
                this._errorMessage = ex.Message;
                return false;
            }
        }
        private bool DeleteFileStatusCheck()
        {
            if (this._status == STATUS.INVALID)
            {
                this._errorCode = ERROR_CODE.DELETE_FILE;
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool DeleteFileProcess(string FileName)
        {
            try
            {
                string mainFile , destFile;
                destFile = Path.Combine(this._filePath, FileName);
                mainFile = Path.Combine(this._filePath, this._fileName + this._fileType);
                if (destFile != mainFile)
                {
                    System.IO.File.Delete(destFile);
                }
                return true;
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DELETE_FILE;
                this._errorMessage = ex.Message;
                return false;
            }
        }
        //End : Delete All

        //Internal Functions
        private void ResetError()
        {
            this._errorCode = ERROR_CODE.NO_ERROR;
            this._errorMessage = string.Empty;
        }
        //Internal Functions

    }
}

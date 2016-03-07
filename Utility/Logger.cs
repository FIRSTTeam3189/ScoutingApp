using System;

namespace Scouty.Utility
{
	/// <summary>
	/// The logger class for logging info
	/// </summary>
	public class Logger
	{
		Type _classType;

		const int TRACE = 0;
		const int INFO = 1;
		const int DEBUG = 2;
		const int WARN = 3;
		const int ERROR = 4;
		const int FATAL = 5;

		public Logger (Type classType)
		{
			if (classType == null)
				throw new ArgumentNullException (nameof (classType));

			_classType = classType;
		}

		void Log(string message, int level){
			string tag = "";
			string className = _classType.Name;

			switch (level) {
			case TRACE:
				tag = "[TRACE]";
				break;
			case INFO:
				tag = "[INFO]";
				break;
			case DEBUG:
				tag = "[DEBUG]";
				break;
			case WARN:
				tag = "[WARN]";
				break;
			case ERROR:
				tag = "[ERROR]";
				break;
			case FATAL:
				tag = "[FATAL]";
				break;
			default:
				tag = "[UNDEFINDED]";
				break;
			}


			if (level > 3)
				Console.Error.WriteLine (tag + " " + className + " " + message);
			else
				Console.WriteLine (tag + " " + className + " " + message);
		}

		void Log(string message, Exception e, int level){
			Log (message + ": " + e.Message, level);
		}

		/// <summary>
		/// Writes out a trace log
		/// </summary>
		/// <param name="message">Message.</param>
		public void Trace(string message){
			Log (message, TRACE);
		}

		/// <summary>
		/// Writes out an info log
		/// </summary>
		/// <param name="message">Message.</param>
		public void Info(string message){
			Log (message, INFO);
		}

		/// <summary>
		/// Writes out a debug log
		/// </summary>
		/// <param name="message">Message.</param>
		public void Debug(string message){
			Log (message, DEBUG);
		}

		/// <summary>
		/// Writes out a warn log
		/// </summary>
		/// <param name="message">Message.</param>
		public void Warn(string message){
			Log (message, WARN);
		}

		/// <summary>
		/// Writes out an error log without an exception
		/// </summary>
		/// <param name="message">Message.</param>
		public void Error(string message){
			Log (message, ERROR);
		}

		/// <summary>
		/// Writes out an error log with an exception
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="e">E.</param>
		public void Error(string message, Exception e){
			Log (message, e, ERROR);
		}

		/// <summary>
		/// Writes out a fatal log without an exception
		/// </summary>
		/// <param name="message">Message.</param>
		public void Fatal(string message){
			Log (message, FATAL);
		}

		/// <summary>
		/// Writes out a fatal log with an exception
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="e">E.</param>
		public void Fatal(string message, Exception e){
			Log (message, e, FATAL);
		}
	}
}


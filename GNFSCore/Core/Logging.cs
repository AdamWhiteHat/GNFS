using System;

namespace GNFSCore
{
	public delegate void LogMessageDelegate(string message);

	public class Logging : IDisposable
	{
		public static bool LoggingEnabled = true;
		public static LogMessageDelegate LogFunction { get; set; } = Console.WriteLine;

		public Logging()
		{
			LoggingEnabled = true;
		}

		public void Dispose()
		{
			LoggingEnabled = false;
		}

		public static void WriteLine()
		{
			WriteLine("");
		}

		public static void WriteLine(string msg)
		{
			if (LoggingEnabled)
			{
				LogFunction.Invoke(msg);
			}
		}

		public static void EnableLogging(bool debug)
		{
			LoggingEnabled = debug;
		}


		private static string superscriptCharacters = "⁰¹²³⁴⁵⁶⁷⁸⁹                              ⁽⁾*⁺ ⁻  ⁰¹²³⁴⁵⁶⁷⁸⁹   ⁼   ᴬᴮꟲᴰᴱꟳᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾꟴᴿ ᵀᵁⱽᵂ         ᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖ ʳˢᵗᵘᵛʷˣʸᶻ";
		public static string GetSuperscript(char character)
		{
			return superscriptCharacters[character].ToString();
		}

		public static string GetSuperscript(int number)
		{
			return superscriptCharacters[number].ToString();
		}

		private static string subscriptCharacters = "₀₁₂₃₄₅₆₇₈₉                              ₍₎ ₊ ₋  ₀₁₂₃₄₅₆₇₈₉   ₌                                   ₐ   ₑ  ₕ  ₖₗₘₙₒₚ  ₛₜ   ₓ  ";
		public static string GetSubcript(char character)
		{
			return subscriptCharacters[character].ToString();
		}
		public static string GetSubscript(int number)
		{
			return subscriptCharacters[number].ToString();
		}
	}
}

using System;

namespace GameEngineChecker.Models.Exceptions
{
	public class ApiErrorException : Exception
	{
		public ApiErrorException(string code)
		{
			Code = code;
		}

		public ApiErrorException(string code, string message) : base(message)
		{
			Code = code;
		}

		public string Code { get; private set; }
	}
}
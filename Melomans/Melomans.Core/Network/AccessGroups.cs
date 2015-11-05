﻿using System.Runtime.Serialization;

namespace MessageRouter.Network
{
	/// <summary>Группы функций</summary>
	[DataContract]
	public enum AccessGroups
	{
		/// <summary>Системный функции</summary>
		[EnumMember]
		System,

		/// <summary>Управления воспроизвидением</summary>
		[EnumMember]
		ManipulatePlayer,

		[EnumMember]
		Player
	}
}

namespace Server
{
	/// <summary>
	/// You may apply explicit overrides `MySettings` overrides here.
	/// This file will never be touched by Memento development.
	/// </summary>
	public static class SettingOverrides
	{
		public static void Initialize()
		{
			MySettings.S_SaveOnCharacterLogout = true;
                        MySettings.S_RunRoutinesAtStartup = false;
                        MySettings.S_Port=8085;
		}
	}
}

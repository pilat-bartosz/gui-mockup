﻿namespace _gui_mockup.DataServer
{
	public class DataItem
	{
		public enum CategoryType
		{
			RED,
			GREEN,
			BLUE,
			LENGTH
		}

		public readonly CategoryType Category;
		public readonly string Description;
		public readonly bool Special;

		public DataItem(CategoryType category, string description, bool special)
		{
			Category = category;
			Description = description;
			Special = special;
		}
	}
}

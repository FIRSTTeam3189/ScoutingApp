using System;
using Scouty.Models.Local;

namespace Scouty.Utility
{
	public static class Statistics
	{
		/// <summary>
		/// Determines if is in category the specified type category.
		/// </summary>
		/// <returns><c>true</c> if is in category the specified type category; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type.</param>
		/// <param name="category">Category.</param>
		public static bool IsInCategory(this DefenseType type, DefenseCategory category){
			if (category == DefenseCategory.A)
				return type == DefenseType.ChevalDeFrise || type == DefenseType.Portcullis;
			else if (category == DefenseCategory.B)
				return type == DefenseType.Moat || type == DefenseType.Ramparts;
			else if (category == DefenseCategory.C)
				return type == DefenseType.Drawbridge || type == DefenseType.SallyPort;
			else if (category == DefenseCategory.D)
				return type == DefenseType.RockWall || type == DefenseType.RoughTerrain;
			else
				return type == DefenseType.LowBar;
		}

		/// <summary>
		/// Gets the sister defense.
		/// </summary>
		/// <returns>The sister defense.</returns>
		/// <param name="type">Type.</param>
		public static DefenseType GetSisterDefense(this DefenseType type){
			if (type == DefenseType.ChevalDeFrise)
				return DefenseType.Portcullis;
			else if (type == DefenseType.Portcullis)
				return DefenseType.ChevalDeFrise;
			else if (type == DefenseType.Drawbridge)
				return DefenseType.SallyPort;
			else if (type == DefenseType.SallyPort)
				return DefenseType.Drawbridge;
			else if (type == DefenseType.Moat)
				return DefenseType.Ramparts;
			else if (type == DefenseType.Ramparts)
				return DefenseType.Moat;
			else if (type == DefenseType.RockWall)
				return DefenseType.RoughTerrain;
			else if (type == DefenseType.RoughTerrain)
				return DefenseType.RockWall;
			else
				return DefenseType.LowBar;
		}

		/// <summary>
		/// Gets the category.
		/// </summary>
		/// <returns>The category.</returns>
		/// <param name="type">Type.</param>
		public static DefenseCategory GetCategory(this DefenseType type){
			if (type == DefenseType.ChevalDeFrise || type == DefenseType.Portcullis)
				return DefenseCategory.A;
			else if (type == DefenseType.Moat || type == DefenseType.Ramparts)
				return DefenseCategory.B;
			else if (type == DefenseType.Drawbridge || type == DefenseType.SallyPort)
				return DefenseCategory.C;
			else if (type == DefenseType.RockWall || type == DefenseType.RoughTerrain)
				return DefenseCategory.D;
			else
				return DefenseCategory.LowBar;
		}
	}
}


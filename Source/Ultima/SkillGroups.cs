﻿#region References
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#endregion

namespace Ultima
{
	public sealed class SkillGroup
	{
		public string Name { get; set; }

		public SkillGroup(string name)
		{
			Name = name;
		}
	}

	public sealed class SkillGroups
	{
		public static List<SkillGroup> List { get; private set; }
		public static List<int> SkillList { get; private set; }
		private static bool unicode;

		static SkillGroups()
		{
			Initialize();
		}

		public static void Initialize()
		{
			var path = Files.GetFilePath("skillgrp.mul");

			List = new List<SkillGroup>();
			SkillList = new List<int>();

			if (path != null)
			{
				using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					using (var bin = new BinaryReader(fs))
					{
						var start = 4;
						var strlen = 17;
						var count = bin.ReadInt32();
						if (count == -1)
						{
							unicode = true;
							count = bin.ReadInt32();
							start *= 2;
							strlen *= 2;
						}

						List.Add(new SkillGroup("Misc"));
						for (var i = 0; i < count - 1; ++i)
						{
							int strbuild;
							_ = fs.Seek(start + (i * strlen), SeekOrigin.Begin);
							var builder2 = new StringBuilder(17);
							if (unicode)
							{
								while ((strbuild = bin.ReadInt16()) != 0)
								{
									_ = builder2.Append((char)strbuild);
								}
							}
							else
							{
								while ((strbuild = bin.ReadByte()) != 0)
								{
									_ = builder2.Append((char)strbuild);
								}
							}
							List.Add(new SkillGroup(builder2.ToString()));
						}
						_ = fs.Seek(start + ((count - 1) * strlen), SeekOrigin.Begin);
						try
						{
							while (bin.BaseStream.Length != bin.BaseStream.Position)
							{
								SkillList.Add(bin.ReadInt32());
							}
						}
						catch // just for safety
						{ }
					}
				}
			}
		}

		public static void Save(string path)
		{
			var mul = Path.Combine(path, "skillgrp.mul");
			using (var fs = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				using (var bin = new BinaryWriter(fs))
				{
					if (unicode)
					{
						bin.Write(-1);
					}
					bin.Write(List.Count);

					foreach (var group in List)
					{
						if (group.Name == "Misc")
						{
							continue;
						}
						byte[] name;
						if (unicode)
						{
							name = new byte[34];
						}
						else
						{
							name = new byte[17];
						}
						if (group.Name != null)
						{
							if (unicode)
							{
								var bb = Encoding.Unicode.GetBytes(group.Name);
								if (bb.Length > 34)
								{
									Array.Resize(ref bb, 34);
								}
								bb.CopyTo(name, 0);
							}
							else
							{
								var bb = Encoding.Default.GetBytes(group.Name);
								if (bb.Length > 17)
								{
									Array.Resize(ref bb, 17);
								}
								bb.CopyTo(name, 0);
							}
						}
						bin.Write(name);
					}
					foreach (var group in SkillList)
					{
						bin.Write(group);
					}
				}
			}
		}
	}
}
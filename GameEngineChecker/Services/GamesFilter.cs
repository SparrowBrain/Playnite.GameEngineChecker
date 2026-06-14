using GameEngineChecker.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineChecker.Services
{
	public class GamesFilter : IGamesFilter
	{
		private readonly HashSet<Guid> _engineTagIds;
		private readonly HashSet<Guid> _pcPlatforms;

		public GamesFilter(IPlayniteAPI api)
		{
			_engineTagIds = api.Database.Tags?.Where(x => x.Name.StartsWith("[Engine]")).Select(x => x.Id).ToHashSet();
			_pcPlatforms = api.Database.Platforms?.Where(x => x.Name.StartsWith("PC") || x.SpecificationId == "pc_windows").Select(x => x.Id).ToHashSet();
		}

		public bool ShouldTheGameBeProcessed(Game game)
		{
			return IsPcPlatform(game) && HasNoEngineTags(game);
		}

		private bool IsPcPlatform(Game game)
		{
			return game.PlatformIds?.Any(id => _pcPlatforms.Contains(id)) ?? false;
		}

		private bool HasNoEngineTags(Game game)
		{
			return game.TagIds?.All(x => !_engineTagIds.Contains(x)) ?? true;
		}
	}
}
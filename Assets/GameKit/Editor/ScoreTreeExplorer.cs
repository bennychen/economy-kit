﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Codeplay
{
    public class ScoreTreeExplorer : ItemTreeExplorer
    {
        public ScoreTreeExplorer()
        {
            _worldToExpanded = new Dictionary<World, bool>();
            InitWorldToExpanded(GameKit.Config.RootWorld);
        }

        public void AddWorld(World world)
        {
            if (!_worldToExpanded.ContainsKey(world))
            {
                _worldToExpanded.Add(world, false);
            }
        }

        public void RemoveWorld(World world)
        {
            if (_worldToExpanded.ContainsKey(world))
            {
                _worldToExpanded.Remove(world);
            }
        }

        private void InitWorldToExpanded(World world)
        {
            _worldToExpanded.Add(world, true);
			foreach (var subWorldID in world.SubWorldsID)
            {
				InitWorldToExpanded(GameKit.Config.GetWorldByID(subWorldID));
            }
        }

        protected override void DoOnSelectItem(IItem item) 
        {
            World w = GameKit.Config.FindWorldThatScoreBelongsTo(item as Score);
            while (w != null)
            {
                if (_worldToExpanded.ContainsKey(w))
                {
                    _worldToExpanded[w] = true;
                }
                else
                {
                    break;
                }
                w = w.Parent;
            }
        }

        protected override void DoExpandAll() 
        { 
            ExpandWorld(GameKit.Config.RootWorld, true);
        }

        protected override void DoCollapseAll() 
        { 
            CollapseWorld(GameKit.Config.RootWorld, true);
        }

        protected override void DoDraw(Rect position, string searchText) 
        { 
            if (string.IsNullOrEmpty(searchText))
            {
                DrawWorldScores(position, GameKit.Config.RootWorld);
            }
            else
            {
                foreach (var world in GameKit.Config.Worlds)
                {
                    foreach (var score in world.Scores)
                    {
                        DrawItemIfMathSearch(searchText, score, position.width);
                    }
                }
            }
        }

        private float DrawWorldScores(Rect position, World world)
        {
            GUILayout.BeginArea(position);
            if (world == null)
            {
                GUILayout.Label("NULL World");
                GUILayout.EndArea();
                return 20;
            }

            float x = 0;
            float y = 0;
            if (_worldToExpanded.ContainsKey(world))
            {
                _worldToExpanded[world] = EditorGUILayout.Foldout(_worldToExpanded[world],
                    new GUIContent(world.ID, Resources.Load("WorldIcon") as Texture), GameKitEditorDrawUtil.FoldoutStyle);
                y += 20;
                if (_worldToExpanded[world])
                {
                    if (world.Scores.Count > 0)
                    {
                        for (int i = 0; i < world.Scores.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(15);
                            Score score = world.Scores[i];
                            if (GUILayout.Button(" " + score.ID, GetItemLeftStyle(score),
                                    GUILayout.Width(position.width - 25), GUILayout.Height(20)))
                            {
                                SelectItem(score);
                            }
                            GUILayout.EndHorizontal();
                            y += 25;
                        }
                    }

                    x += 20;

					if (world.SubWorldsID.Count > 0)
                    {
						foreach (var subWorldID in world.SubWorldsID)
                        {
                            y += DrawWorldScores(new Rect(x, y,
								position.width - 20, position.height), GameKit.Config.GetWorldByID(subWorldID));
                        }
                    }
                }
            }

            GUILayout.EndArea();
            return y;
        }

        private void ExpandWorld(World world, bool resursive = false)
        {
            if (_worldToExpanded.ContainsKey(world))
            {
                _worldToExpanded[world] = true;
                if (resursive)
                {
					foreach (var subWorldID in world.SubWorldsID)
                    {
						ExpandWorld(GameKit.Config.GetWorldByID(subWorldID), true);
                    }
                }
            }
        }

        private void CollapseWorld(World world, bool resursive = false)
        {
            if (_worldToExpanded.ContainsKey(world))
            {
                _worldToExpanded[world] = false;
                if (resursive)
                {
					foreach (var subWorldID in world.SubWorldsID)
                    {
						ExpandWorld(GameKit.Config.GetWorldByID(subWorldID), false);
                    }
                }
            }
        }

        private Dictionary<World, bool> _worldToExpanded;
    }
}

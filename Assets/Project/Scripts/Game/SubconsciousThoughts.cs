namespace Game
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class SubconsciousThoughts
    {
        private static readonly List<List<List<string>>> levelThoughts = new List<List<List<string>>>()
        {
        // Level 0
        new List<List<string>> {
            new List<string> {
                "Boots stick to the floor.",
                "Not mud... something older.",  // Многоточие сохранено как часть мысли
                "Who lights these torches?",
                "No one's been here for ages."
            },
            new List<string> {
                "Moss grows upward here.",
                "Oil lamps burn black...",      // Многоточие как пауза, не разбито
                "Wrong. All wrong."
            }
        },
    
        // Level 1
        new List<List<string>> {
            new List<string> {
                "Stones whisper.",
                "Armor feels lighter... or my bones heavier?"  // Многоточие внутри одной мысли
            },
            new List<string> {
                "How many steps now?",
                "My count gets tangled."
            },
            new List<string> {
                "Was that my shadow?",
                "It moved... differently."     // Многоточие как часть предложения
            }
        },

        // Level 2
        new List<List<string>> {
            new List<string> {
                "Breath fogs...",
                "But the air's warm.",
                "Why am I cold?"
            },
            new List<string> {
                "Names slip my mind.",
                "Sword hilt—familiar... yet foreign."  // Многоточие внутри одной строки
            },
            new List<string> {
                "The walls... veins?",
                "No. Carvings.",
                "Must be carvings."
            },
            new List<string> {
                "Teeth in the steps.",
                "Stone teeth.",
                "Chewing... what?" }
        },

        // Level 3
        new List<List<string>> {
            new List<string> {
                "Blood rust? No.",
                "The metal bleeds."
            },
            new List<string> {
                "My reflection...",
                "Eyes aren't mine.",
                "Keep moving."
            },
            new List<string> {
                "Teeth in the steps.",
                "Stone teeth.",
                "Chewing... what?"
            }
        },

        // Level 4
        new List<List<string>> {
            new List<string> {
                "Fingers... too long.",
                "When did that—"
            },
            new List<string> {
                "Hungry.",
                "So hungry.",
                "Not my hunger."
            },
            new List<string> {
                "The dark has a heartbeat.",
                "It's using mine."
            }
        },

        // Level 5
        new List<List<string>> {
            new List<string> {
                "This is the end...",
                "no really",
                "no more content guys =)",
                "rate this game plz *heart*",
            },
        }
        };
        public static List<string> GetRandomThought(int currentLevel)
        {
            int clampedLevel = Mathf.Clamp(currentLevel - 1, 0, levelThoughts.Count - 1);
            var levelData = levelThoughts[clampedLevel];
            return levelData[Random.Range(0, levelData.Count)];
        }
    }
}
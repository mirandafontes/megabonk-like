using System;
using System.Collections.Generic;
using System.Linq;


namespace Event
{
    public static class EventBus
    {
        // Estrutura genérica armazenar os delegates de um tipo T específico.
        // Utilizar uma struct implica em uma tipagem e controle básico
        // sobre as actions registradas. Fica mais fácil de identificar
        // onde e quando estão sendo utilizadas.
        private static class EventHub<T> where T : struct
        {
            public static readonly List<Action<T>> Actions = new List<Action<T>>();
        }

        public static void Subscribe<T>(Action<T> action) where T : struct
        {
            if (action == null)
            {
                return;
            }

            // Adiciona o delegate à lista específica do tipo T
            EventHub<T>.Actions.Add(action);
        }

        public static void Unsubscribe<T>(Action<T> action) where T : struct
        {
            if (action == null)
            {
                return;
            }

            // Remove o delegate da lista específica do tipo T
            EventHub<T>.Actions.Remove(action);
        }

        public static void Publish<T>(T eventData) where T : struct
        {
            // Uma copia da lista evitar o 
            // erro de modificação da lista durante o for
            var listeners = EventHub<T>.Actions.ToList();

            foreach (var action in listeners)
            {
                action?.Invoke(eventData);
            }
        }
    }
}
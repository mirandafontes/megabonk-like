using UnityEngine;

namespace Spawn
{
    //Poderíamos utilizar um DI ou um Strategy Pattern
    //para garantir a extensão dos métodos de spawn.
    //Entretanto, preferi manter a simplicidade por enquanto.
    public interface ISpawner
    {
        public Vector3 GetSpawnPosition(Transform playerTransform);
    }
}
using UnityEngine;

namespace CarControllerwithShooting
{
    public class NaturalAI : MonoBehaviour
    {
        public int Health = 100;
        public bool isExplosive;
        public bool destroyParentOnDeath;
        private bool isExploded = false;
        public Collider MainCollider;

        public GameObject explosionEffect;
        private GameObject particleParent;

        private void Start()
        {
            SetupParticleParent();
        }

        private void SetupParticleParent()
        {
            if (destroyParentOnDeath && transform != null)
            {
                particleParent = new GameObject("Particle Parent");
                // Create a list of children first
                var children = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                {
                    children[i] = transform.GetChild(i);
                }

                // Now reparent all children
                foreach (var child in children)
                {
                    if (child != null)
                    {
                        child.SetParent(particleParent.transform);
                    }
                }
            }
        }

        public void GetDamage(int Damage)
        {
            Health = Health - Damage;
            if (Health <= 0 && !isExploded)
            {
                if (destroyParentOnDeath)
                {
                    gameObject.SetActive(false);
                    Explode(particleParent);
                }
                else
                {
                    Explode(gameObject);
                }
            }
        }

        private void Explode(GameObject targetObject)
        {
            isExploded = true;
            foreach (var meshCollider in targetObject.GetComponentsInChildren<MeshCollider>())
            {
                meshCollider.enabled = true;
            }
            // Let's Explode
            foreach (var rigidbody in targetObject.GetComponentsInChildren<Rigidbody>())
            {
                rigidbody.useGravity = true;
                rigidbody.isKinematic = false;
                rigidbody.AddExplosionForce(Random.Range(4, 15), rigidbody.transform.position, Random.Range(4, 10), Random.Range(1, 2), ForceMode.Impulse);
                rigidbody.AddRelativeTorque(new Vector3(Random.Range(-4, 4), Random.Range(-4, 4), Random.Range(-4, 4)), ForceMode.Impulse);
                Destroy(rigidbody.gameObject, 10);
            }

            if (isExplosive || explosionEffect != null)
            {
                Instantiate(explosionEffect, gameObject.transform.position, Quaternion.identity);
            }


            if (RadarSystem.Instance != null)
                RadarSystem.Instance.RemoveTarget(targetObject.gameObject);
            // Let's Destroy itself
            Destroy(gameObject, 6);
        }
    }
}

using UnityEngine;

namespace AnimationHandling
{

    public class Helper : MonoBehaviour
    {
        [Range(0, 1)]
        public float vertical;
        [Range(0, 1)]
        public float horizontal;

        /***
        0 = unarmed
        1 = one handed sword
        2 = two handed sword
        3 = sword and shield
        4 = bow
        ***/
        [Range(0,4)]
        public int loadout;

        [SerializeField()]
        Animator animator;

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.W))
                {
                    vertical = 1;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    horizontal = 1;
                }
            }
            else if (Input.GetKey(KeyCode.W))
            {
                vertical = 0.5f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                horizontal = 0.5f;
            }
            else
            {
                //horizontal = 0;
                //vertical = 0;
            }
            animator.SetFloat("Vertical", vertical);
            animator.SetFloat("Horizontal", horizontal);
            animator.SetInteger("Loadout", loadout);
        }
    }
}
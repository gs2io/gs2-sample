using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Realtime
{
    public class OtherPlayer : MonoBehaviour
    {
        public float moveSpeed;
        private Vector2 _vector;
        private Text _textMesh;
        private int _frame = 0;
        public int keyFrameInterval = 60 * 5;
        
        private void Start()
        {
            _textMesh = GetComponent<Text>();
            
            _vector.Set(
                transform.position.x,
                transform.position.y
            );
            
            gameObject.SetActive(true);
        }

        void Update () {
            var step = moveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(
                transform.position,
                _vector, 
                step
            );
        }

        public void Deserialize(byte[] data)
        {
            var pos = 0;
            if (_frame++ % keyFrameInterval == 0)
            {
                var position = transform.position;
                position.x = BitConverter.ToSingle(data, pos);
                position.y = BitConverter.ToSingle(data, pos + 4);
                transform.position = position;
            }

            pos += 8;
            _vector.x = BitConverter.ToSingle(data, pos);
            _vector.y = BitConverter.ToSingle(data, pos + 4);
            pos += 8;
            if (_textMesh == null)
            {
                
            }
            else
            {
                try
                {
                    _textMesh.text = Encoding.UTF8.GetString(data, pos, data.Length-pos);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
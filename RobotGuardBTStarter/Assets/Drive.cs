using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour {

    // Velocidade de movimentação do objeto
	float speed = 20.0F;
    // Velocidade de rotação do objeto
    float rotationSpeed = 120.0F;
    // Prefab do projétil disparado
    public GameObject bulletPrefab;
    // Origem do projétil disparado
    public Transform bulletSpawn;

    void Update() {
        //Obtendo comando que gerará a movimentação do objeto
        float translation = Input.GetAxis("Vertical") * speed;
        //Obtendo comando que gerará a rotação do objeto
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
        //Multiplicando o valor de movimentação e rotação por Time.deltaTime, para manter uma velocidade constante independente do desempenho
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;

        //Executando movimentação com base no eixo Z do objeto, movimentando-o para sua frente ou trás
        transform.Translate(0, 0, translation);
        //Rotacionando o objeto em torno de seu eixo Y, para que ele vire para os lados
        transform.Rotate(0, rotation, 0);

        //Executa função ao pressionar a tecla de espaço
        if(Input.GetKeyDown("space")) 
        {
            //Instanciando projétil de disparo
            GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            //Adiciona força ao projétil, o movimentando para sua frente
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward*2000);
        }
    }
}

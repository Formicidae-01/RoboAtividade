using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    //Corpo do objeto do jogador
    public Transform player;
    //Origem do projétil disparado
    public Transform bulletSpawn;
    //Barra de vida do objeto
    public Slider healthBar; 
    //Projétil que é disparado por objeto  
    public GameObject bulletPrefab;

    //Componente navMesh, que executa a movimentação do objeto até um caminho específico
    NavMeshAgent agent;
    //Destino da movimentação
    public Vector3 destination; // The movement destination.
    //Alvo da rotação (mira)
    public Vector3 target;      // The position to aim to.
    //Valor de vida do objeto
    float health = 100.0f;
    //Velocidade de rotação do objeto
    float rotSpeed = 5.0f;

    //Distância máxima de visibilidade do objeto, de quão longe ele pode de detectar o jogador
    float visibleRange = 80.0f;
    //Distância que o jogador precisa estar desse objeto para atirar
    float shotRange = 40.0f;

    void Start()
    {
        //Obtendo o navmesh no início da execução
        agent = this.GetComponent<NavMeshAgent>();
        //Determinando a distância que o personagem precisa estar do jogador para deixar de se mover
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        //Invocando constantemente método que verifica a vida do objeto e a impede de ficar menor que 100
        InvokeRepeating("UpdateHealth",5,0.5f);
    }

    void Update()
    {
        //Gerando a posição na qual a barra de vida deve permanecer
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        //Sincronizando a barra de vida com o valor
        healthBar.value = (int)health;
        //Atualizando a barra de vida do objeto, para que fique sincronizada
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
    }

    //Método que aumenta a vida do objeto caso ela esteja menor que 100
    void UpdateHealth()
    {
       if(health < 100)
        health ++;
    }

    //Método de colisão
    void OnCollisionEnter(Collision col)
    {
        //Subtrai 10 da vida do objeto caso tenha colidido com uma bala (Determinada pela tag "bullet")
        if(col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void PickRandomDestination()
    {
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100,100));
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void PickDestination(int x, int z)
    {
        //Cria um vetor que serve como destino de movimentação do agente navMesh
        Vector3 dest = new Vector3(x,0,z);
        //Aplica o vetor acima ao agente navMesh
        agent.SetDestination(dest);
        //Conclui a tarefa
        Task.current.Succeed();
    }
    
    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void MoveToDestination()
    {
        //Exibe a quanto tempo a tarefa está sendo executada (se estiver sendo executada)
        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
        }

        //Conclui a tarefa caso o personagem esteja próximo do local final e não esteja escolhendo um próximo caminho
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void TargetPlayer()
    {
        //Determina o vetor "target" como a posição do jogador, significa que o esse objeto estará apontando ao jogador
        target = player.transform.position;
        //Conclui a tarefa
        Task.current.Succeed();
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public bool Fire()
    {
        //Instancia o projétil
        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        //Adiciona força ao projétil, fazendo com que ele avance
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);
        //Retorna um valor verdadeiro, indicando que o método foi realizado
        return true;
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void LookAtTarget()
    {
        //Cria um vetor cujo valor é a subtração da posição alvo e a posição desse objeto
        Vector3 direction = target - transform.position;
        //Rotaciona esse objeto até o alvo através de uma interpolação
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);
        //Executa função enquanto a tarefa está sendo executada
        if (Task.isInspected)
        {
            //Exibe a quanto tempo a tarefa está sendo executada
            Task.current.debugInfo = string.Format("angle={0}", Vector3.Angle(transform.forward, direction));

            //Executa método caso a rotação esteja cumprida ("Vector3.Angle" é bem similar a "Vector3.Distance")
            if (Vector3.Angle(transform.forward, direction) < 5.0f)
            {
                //Conclui a tarefa
                Task.current.Succeed();
            }
        }
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    bool SeePlayer()
    {
        //Vetor que será utilizado como alvo de um raycast
        Vector3 distance = player.transform.position - transform.position;
        //Variável que guarda as informações de um raycast
        RaycastHit hit;
        //boolean que determina se o objeto está observando ou não uma parede
        bool seeWall = false;
        //Desenha uma linha que vai da posição do objeto até o destino do raycast, criando um traço visual (somente visível com gizmos ativados)
        Debug.DrawRay(transform.position, distance, Color.red);
        //Dispara um raycast da posição do jogador até a posição do vetor "distance", executa método se o raio atingir algum colisor
        if (Physics.Raycast(transform.position, distance, out hit))
        {
            //Se o objeto atingido for uma parede (identificado pela tag "Wall", o boolean "seeWall" se torna verdadeiro)
            if (hit.collider.gameObject.tag == "wall")
            {
                seeWall = true;
            }
        }

        //Exibe a quanto tempo a tarefa está sendo executada
        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("wall={0}", seeWall);
        }
        
        //Se o tamanho do vetor for menor que a distância máxima de visão e esse objeto não estiver olhando para uma parede, o valor retornado é verdadeiro, significa que o objeto viu o personagem
        if (distance.magnitude < visibleRange && !seeWall)
        {
            return true;
        }
        //Se a condição acima não for cumprida, o valor retornado é falso, significa que esse objeto não vê o jogador
        else;
        {
            return false;
        }
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    bool Turn(float angle)
    {
        //Cria uma variável que será o alvo para qual o objeto irá rotacionar
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
        //Determina o alvo da rotação igual ao valor acima
        target = p;
        //Retorna um valor verdadeiro
        return true;
    }
}


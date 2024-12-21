using System.Collections.Generic;
using Omni.Core;
using UnityEngine;

public class Carrossel : ServiceBehaviour
{
    [Header("Configurações")]
    public float rotationSpeed = 5f; // Velocidade de rotação
    public List<Transform> items; // Lista de objetos no carrossel

    public int currentIndex = 0; // Índice do item atualmente focado
    private bool isRotating = false; // Controla se está no meio de uma rotação
    private Quaternion targetRotation; // Rotação alvo do carrossel
    
    public void Roleta(int i)
    {
        currentIndex = (currentIndex + i) % items.Count;
        
        RotateToItem(currentIndex);
    }
    void Update()
    {
        // Rotação suave
        if (isRotating)
        {
            transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    /// <summary>
    /// Gira o carrossel para alinhar o item especificado com a câmera.
    /// </summary>
    /// <param name="index">Índice do item na lista.</param>
    private void RotateToItem(int index)
    {
        // Calcula o ângulo para alinhar o item atual com a câmera
        float anglePerItem = 360f / items.Count;
        float targetAngleY = -anglePerItem * index;

        // Define a rotação alvo apenas no eixo Y
        targetRotation = Quaternion.Euler(0f, targetAngleY, 0f);
        isRotating = true;
    }

    /// <summary>
    /// Adiciona um novo item ao carrossel.
    /// </summary>
    /// <param name="item">Transform do item para adicionar.</param>
    public void AddItem(Transform item)
    {
        if (!items.Contains(item))
        {
            items.Add(item);
        }
    }
}

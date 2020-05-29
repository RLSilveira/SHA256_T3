using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SHA256_T3
{
    class Program
    {
        static void Main(string[] args)
        {
            const int BUFFER_SIZE = 1024;

            //var video = Path.Combine("data", "FuncoesResumo - SHA1.mp4");
            var video = Path.Combine("data", "FuncoesResumo - Hash Functions.mp4");

            using (SHA256 mySHA256 = SHA256.Create())
            {
                //Obtem informações do arquivo
                FileInfo fInfo = new FileInfo(video);
                try
                {
                    // *** ETAPA 1 - divisão do arquivo em blocos ***

                    // Calcula a quantidade de blocos e o tamanho do último bloco
                    var lastBlockLength = fInfo.Length % BUFFER_SIZE;
                    var estimatedBlocks = fInfo.Length / BUFFER_SIZE + (lastBlockLength == 0 ? 0 : 1);

                    // Pilha para guardar os blocos do arquivo
                    var stackFileBlocks = new Stack<byte[]>((int)estimatedBlocks);

                    // Leitura e split do arquivo para pilha
                    using (Stream input = fInfo.OpenRead())
                    {
                        while (input.Position < input.Length)
                        {
                            var bufferSize = (int)Math.Min(input.Length - input.Position, BUFFER_SIZE);
                            byte[] buffer = new byte[bufferSize];
                            input.Read(buffer, 0, bufferSize);
                            stackFileBlocks.Push(buffer);
                        }
                    }
                    
                    
                    // *** ETAPA 2 - cálculo dos hashes ***

                    // pilha para guardar os hashes na ordem correta
                    var stackHashes = new Stack<byte[]>((int)estimatedBlocks);

                    // Processa pilha para gerar hashes
                    byte[] lastHash = null;
                    while (stackFileBlocks.Count > 0)
                    {
                        var fileBlock = stackFileBlocks.Pop();

                        // Último bloco não possui hashe anterior para concatenar
                        if (lastHash == null)
                        {
                            var lastFileBlock = mySHA256.ComputeHash(fileBlock);
                            stackHashes.Push(lastFileBlock);
                            lastHash = lastFileBlock;

                            fileBlock = stackFileBlocks.Pop();
                        }

                        // concatenar bloco do arquivo com hash anterior
                        var fileBlockWithHash = new byte[fileBlock.Length + lastHash.Length];

                        fileBlock.CopyTo(fileBlockWithHash, 0);
                        lastHash.CopyTo(fileBlockWithHash, fileBlock.Length);


                        // computar hash
                        var hash = mySHA256.ComputeHash(fileBlockWithHash);

                        stackHashes.Push(hash);

                        // atualizar último hash para próxima iteração
                        lastHash = hash;
                    }


                    // *** ETAPA 3 - gerar arquivo texto como saída do algoritmo ***

                    var sb = new StringBuilder((int)estimatedBlocks);
                    int i = 0;
                    while (stackHashes.Count > 0)
                    {
                        var str = ByteArrayToString(stackHashes.Pop());
                        sb.AppendLine($"{i++}:\t {str}");
                    }

                    // resultados gravados na pasta bin\Debug\netcoreapp3.1\output
                    Directory.CreateDirectory("output");
                    var outputFile = Path.Combine("output", Path.GetFileNameWithoutExtension(fInfo.Name) + ".txt");
                    using (var swriter = new StreamWriter(outputFile, false))
                    {
                        swriter.Write(sb.ToString());
                    }

                }
                catch (IOException e)
                {
                    Console.WriteLine($"I/O Exception: {e.Message}");
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine($"Access Exception: {e.Message}");
                }
            }
        }

        // Display the byte array in a readable format.
        private static void PrintByteArray(byte[] array)
        {
            foreach (var t in array)
            {
                Console.Write($"{t:X2}");
            }

            Console.WriteLine("");

        }

        private static string ByteArrayToString(byte[] array)
        {
            return array.Aggregate("", (current, t) => current + $"{t:X2}");
        }

    }
}
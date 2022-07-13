using CornyFlakezPlugin;
using LSPD_First_Response;
using System;
using System.Collections.Generic;
using System.Xml;
using static Main.CalloutCommons;

namespace DebuggingProject
{
    class Program
    {
        public class ListNode
        {
            public int val;
            public ListNode next;
            public ListNode(int val = 0, ListNode next = null)
            {
                this.val = val;
                this.next = next;
            }
        }

        public static class Solution
        {
            public static ListNode AddTwoNumbers(ListNode l1, ListNode l2)
            {
                int remainder = 0;
                ListNode resultFirstNode = new ListNode();
                ListNode resultNode = resultFirstNode;
                while (l1 != null || l2 != null || resultNode.val != 0) {
                    l1 ??= new ListNode();
                    l2 ??= new ListNode();
                    resultNode.val += (l1.val + l2.val) % 10;
                    remainder = (int)Math.Floor((l1.val + l2.val) / 10f) + (int)Math.Floor(resultNode.val / 10f);
                    resultNode.val %= 10;
                    if (l1.next == null && l2.next == null && remainder == 0)
                        break;
                    resultNode.next = new ListNode(remainder);
                    resultNode = resultNode.next;
                    l1 = l1.next;
                    l2 = l2.next;
                }
                return resultFirstNode;
            }
        }

        private static void Main(string[] args)
        {
            ListNode l1 = new(2, new(4, new(3)));
            ListNode l2 = new(5, new(6, new(4)));
            ListNode resultNode = Solution.AddTwoNumbers(l1, l2);
            while (resultNode != null)
            {
                Console.Write(resultNode.val);
                resultNode = resultNode.next;
            }
        }
    }
}

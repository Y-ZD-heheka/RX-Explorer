﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RX_Explorer.Class
{
    public sealed class OperationListDeleteUndoModel : OperationListUndoModel
    {
        public override string FromDescription => string.Empty;

        public override string ToDescription
        {
            get
            {
                if (UndoFrom.Length > 5)
                {
                    return $"{Globalization.GetString("TaskList_To_Label")}: {Environment.NewLine}{string.Join(Environment.NewLine, UndoFrom.Take(5))}{Environment.NewLine}({UndoFrom.Length - 5} {Globalization.GetString("TaskList_More_Items")})...";
                }
                else
                {
                    return $"{Globalization.GetString("TaskList_To_Label")}: {Environment.NewLine}{string.Join(Environment.NewLine, UndoFrom)}";
                }
            }
        }

        public string[] UndoFrom { get; }

        public override bool CanBeCancelled => true;

        public override async Task PrepareSizeDataAsync(CancellationToken Token)
        {
            ulong TotalSize = 0;

            foreach (FileSystemStorageItemBase Item in await FileSystemStorageItemBase.OpenInBatchAsync(UndoFrom))
            {
                if (Token.IsCancellationRequested)
                {
                    break;
                }

                switch (Item)
                {
                    case FileSystemStorageFolder Folder:
                        {
                            TotalSize += await Folder.GetFolderSizeAsync(Token);
                            break;
                        }
                    case FileSystemStorageFile File:
                        {
                            TotalSize += File.Size;
                            break;
                        }
                }
            }

            Calculator = new ProgressCalculator(TotalSize);
        }

        public OperationListDeleteUndoModel(string[] UndoFrom, EventHandler OnCompleted = null, EventHandler OnErrorThrow = null, EventHandler OnCancelled = null) : base(OnCompleted, OnErrorThrow, OnCancelled)
        {
            if (UndoFrom.Any((Path) => string.IsNullOrWhiteSpace(Path)))
            {
                throw new ArgumentNullException(nameof(UndoFrom), "Parameter could not be empty or null");
            }

            this.UndoFrom = UndoFrom;
        }
    }
}

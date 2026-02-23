using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.SchedulerDefinitions;

namespace PT.PackageDefinitions
{
    #region Undo action class
    /// <summary>
    /// Class for use in creating DataTables for showing Undo and Redo actions.
    /// </summary>
    public class UndoAction
    {
        //These three are visible, in this sequence.
        public const string ACTION = "Action";
        public const string USER = "User";
        public const string LOCALTIMESTAMP = "LocalTimeStamp";
        public const string UNDO_SET_ID = "UndoSetId";
        public const string REDO_SPAN = "RedoSpan";
        public const string TRAN_NBR = "TransNbr";
        public const string CAN_UNDO = "CanUndo";
        public const string c_canUndoSingleKey = "CanUndoSingle";
        public const string PLAY = "Play";
        public const string TRAN_UNIQUEID = "TransmissionId";
        public const string TRAN_ID = "Id";
        public const string TRAN_TIME = "TransmissionTimeTicks";

        public UndoAction(ulong a_undoSetId, string a_action, long a_userId, DateTime a_timeStamp, TimeSpan a_redoSpan, ulong a_transNbr, bool a_canUndo, bool a_canUndoSingle, bool a_play, ulong a_tranUniqueId, string a_tranmissionId)
        {
            m_undoSetId = a_undoSetId;
            m_action = a_action;
            m_user = a_userId;
            m_timeStamp = a_timeStamp;
            m_redoSpan = a_redoSpan;
            m_transNbr = a_transNbr;
            m_canUndo = a_canUndo;
            m_canUndoSingle = a_canUndoSingle;
            m_play = a_play;
            m_transUniqueId = a_tranUniqueId;
            m_transmissionId = a_tranmissionId;
        }

        #region Undo action properties
        private ulong m_undoSetId;

        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public ulong UndoSetId
        {
            get => m_undoSetId;
            set => m_undoSetId = value;
        }

        private ulong m_transUniqueId;

        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public ulong TransUniqueId
        {
            get => m_transUniqueId;
            set => m_transUniqueId = value;
        }

        private string m_action;

        [System.ComponentModel.ParenthesizePropertyName(true)]
        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public string Action
        {
            get => m_action;
            set => m_action = value;
        }

        private long m_user;

        [System.ComponentModel.ParenthesizePropertyName(true)]
        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public long User
        {
            get => m_user;
            set => m_user = value;
        }

        private DateTime m_timeStamp;

        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public DateTime TimeStamp
        {
            get => m_timeStamp;
            set => m_timeStamp = value;
        }

        private TimeSpan m_redoSpan;

        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public TimeSpan RedoSpan
        {
            get => m_redoSpan;
            set => m_redoSpan = value;
        }

        private ulong m_transNbr;

        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public ulong TransNbr
        {
            get => m_transNbr;
            set => m_transNbr = value;
        }

        private bool m_canUndo;

        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public bool CanUndo
        {
            get => m_canUndo;
            set => m_canUndo = value;
        }

        private bool m_play;

        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public bool Play
        {
            get => m_play;
            set => m_play = value;
        }

        private bool m_canUndoSingle;

        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public bool CanUndoSingle
        {
            get => m_canUndoSingle;
            set => m_canUndoSingle = value;
        }

        private string m_transmissionId;

        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public string TransmissionId
        {
            get => m_transmissionId;
            set => m_transmissionId = value;
        }
        #endregion
    }
    #endregion
}

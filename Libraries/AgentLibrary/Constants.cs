using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentLibrary
{
    public class Constants
    {
        public const char TAG_LEFT = '<';
        public const char TAG_RIGHT = '>';
        public const char MULTIPLE_WILDCARD_INDICATOR = '+';
        public const char EMPTY_PATTERN_TERM_LEFT = '{';
        public const char EMPTY_PATTERN_TERM_RIGHT = '}';

        public const string LTM_NAME = "LongTermMemory";
        public const string STM_NAME = "ShortTermMemory";
        public const string LTM_ITEM_PREFIX = "L";
        public const string STM_ITEM_PREFIX = "S";
        public const string GROUP_ID_PREFIX = "A";

        public const string LTM_ID_FORMAT = "00000000";  // 20190409
        public const string STM_ID_FORMAT = "000000";
        public const string GROUP_ID_FORMAT = "000";
        public const string LOCAL_ID_FORMAT = "00000";
        
        public const string ACTION_ITEM_ID_TAG = "ActionItemID";
        public const string ACTION_ITEM_TYPE_TAG = "ActionItemType";

        public const string INPUT_ITEM_VALUE = "InputItem";
        public const string COGNITIVE_ITEM_VALUE = "CognitiveItem";
        public const string OUTPUT_ITEM_VALUE = "OutputItem";
        public const string ITEM_POINTER_TAG = "Item";

        public const string HISTORY_ITEM_MESSAGE_TAG = "Message";

        public const string DEFAULT_CONFUSION_HANDLER_TAG = "DefaultConfusionHandler";

        public const string DYNAMIC_INFORMATION_TAG = "DynamicInformation";
        public const char DYNAMIC_INFORMATION_ITEM_LEFT_TAG = '<';

        public const string CATEGORY_TAG = "category";
        public const string NAME_TAG = "name";

        // 20190322
        public const string USER_SENTENCE_COMPLEXITY_TAG = "UserSentenceComplexity";
        public const string USER_EMA_SENTENCE_COMPLEXITY_TAG = "UserEMASentenceComplexity";
        public const string AGENT_SENTENCE_COMPLEXITY_TAG = "AgentSentenceComplexity";

        // 20190624
        public const char CONTEXT_SPLIT_MARKER = ':';

        // 20191229
        public const string DISPLAY_OUTPUT_SEPARATOR = "|";
    }
}

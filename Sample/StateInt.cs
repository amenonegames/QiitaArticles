    public struct StateInt :IEquatable<StateInt>
    {
        private readonly CharacterType _characterType;

        private readonly int _stateNo;
        
        public StateInt(MaryState state)
        {
            _characterType = CharacterType.Imposter;
            _stateNo = (int)state;
        }
        
        public StateInt(RoleModelState state)
        {
            _characterType = CharacterType.RoleModel;
            _stateNo = (int)state;
        }
        
        public StateInt(BlackCatState state)
        {
            _characterType = CharacterType.BlackCat;
            _stateNo = (int)state;
        }
        
        public StateInt(CallPartnerState state)
        {
            _characterType = CharacterType.Partner;
            _stateNo = (int)state;
        }
        
        public static implicit operator MaryState (StateInt stateInt)
        {
            Assert.IsTrue(stateInt._characterType == CharacterType.Imposter);
            return (MaryState)stateInt._stateNo;
        }
        
        public static implicit operator RoleModelState (StateInt stateInt)
        {
            Assert.IsTrue(stateInt._characterType == CharacterType.RoleModel);
            return (RoleModelState)stateInt._stateNo;
        }
        
        public static implicit operator BlackCatState (StateInt stateInt)
        {
            Assert.IsTrue(stateInt._characterType == CharacterType.BlackCat);
            return (BlackCatState)stateInt._stateNo;
        }
        
        public static implicit operator CallPartnerState (StateInt stateInt)
        {
            Assert.IsTrue(stateInt._characterType == CharacterType.Partner);
            return (CallPartnerState)stateInt._stateNo;
        }
        
        public static implicit operator int (StateInt stateInt)
        {
            return stateInt._stateNo;
        }
        



        public bool Equals(StateInt other)
        {
            return _characterType == other._characterType && _stateNo == other._stateNo;
        }

        public override bool Equals(object obj)
        {
            return obj is StateInt other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)_characterType, _stateNo);
        }
        
        public static bool operator ==(StateInt left, StateInt right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StateInt left, StateInt right)
        {
            return !(left == right);
        }
        
        private enum CharacterType
        {
            Imposter,
            RoleModel,
            BlackCat,
            Partner
        }

    }
    
    
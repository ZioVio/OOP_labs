"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const Fighter_1 = require("./Fighter");
const UserStorage_1 = __importDefault(require("../storage/UserStorage"));
const FighterStorage_1 = __importDefault(require("../storage/FighterStorage"));
const config_1 = require("../config/config");
class User {
    constructor(id, nickName, stateValue = UserStateEnum.UserDefault, bufferFighterType = Fighter_1.FighterType.FighterAwesome) {
        this.id = id;
        this.nickName = nickName;
        this.stateValue = stateValue;
        this.bufferFighterType = bufferFighterType;
        this.setState(stateValue);
    }
    setState(state) {
        if (state instanceof UserState) {
            this.state = state;
            UserStorage_1.default.setUserStateValue(this, this.state.enumValue);
        }
        else {
            this.state = this.getStateValueFromEmun(state);
            UserStorage_1.default.setUserStateValue(this, state);
        }
    }
    clone() {
        return new User(this.id, this.nickName, this.stateValue, this.bufferFighterType);
    }
    getStateValueFromEmun(state) {
        switch (state) {
            case UserStateEnum.UserSelectingFighterType: return new UserSelectingFighterTypeState(this);
            case UserStateEnum.UserEnteringFighterName: return new UserEnteringFighterNameState(this);
            default: return new UserDefaultState(this);
        }
    }
    getFighters(page = 1) {
        if (page < 0)
            page = 0;
        // todo PROXY
        return FighterStorage_1.default.getUserFighters(this.id)
            .slice((page - 1) * config_1.config.TELEGRAM_MESSAGES_PER_SECOND);
    }
    toJSON() {
        const copy = { ...this, state: undefined };
        return copy;
    }
}
exports.User = User;
var UserStateEnum;
(function (UserStateEnum) {
    UserStateEnum[UserStateEnum["UserDefault"] = 0] = "UserDefault";
    UserStateEnum[UserStateEnum["UserSelectingFighterType"] = 1] = "UserSelectingFighterType";
    UserStateEnum[UserStateEnum["UserEnteringFighterName"] = 2] = "UserEnteringFighterName";
})(UserStateEnum = exports.UserStateEnum || (exports.UserStateEnum = {}));
class UserState {
    constructor(user) {
        this.user = user;
    }
}
exports.UserState = UserState;
class UserDefaultState extends UserState {
    constructor() {
        super(...arguments);
        this.enumValue = UserStateEnum.UserDefault;
    }
    canSelectFighterType() {
        return true;
    }
    canEnterFighterName() {
        return false;
    }
}
exports.UserDefaultState = UserDefaultState;
class UserSelectingFighterTypeState extends UserState {
    constructor() {
        super(...arguments);
        this.enumValue = UserStateEnum.UserSelectingFighterType;
    }
    canSelectFighterType() {
        return true;
    }
    canEnterFighterName() {
        return false;
    }
}
exports.UserSelectingFighterTypeState = UserSelectingFighterTypeState;
class UserEnteringFighterNameState extends UserState {
    constructor() {
        super(...arguments);
        this.enumValue = UserStateEnum.UserEnteringFighterName;
    }
    canSelectFighterType() {
        return false;
    }
    canEnterFighterName() {
        return true;
    }
}
exports.UserEnteringFighterNameState = UserEnteringFighterNameState;
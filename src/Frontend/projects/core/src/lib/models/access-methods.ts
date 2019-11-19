export enum ResourceAccessMethods {
    None = 0,
    Read = 1 << 0,
    Modify = 1 << 1,
    Delete = 1 << 2,
    Add = 1 << 3,
}

export enum DataAccessMethods {
    None = 0,
    Read = 1 << 0,
    Write = 1 << 1,
}

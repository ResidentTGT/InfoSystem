export class SearchFilters {
    public departments: number[] = [];
    public managers: number[] = [];
    public brands: number[] = [];
    public seasons: number[] = [];
    public contractor: string;
    public dealId: number;
    public discountFrom: number;
    public discountTo: number;
    public createDateFrom: Date;
    public createDateTo: Date;
    public loadDateFrom: Date;
    public loadDateTo: Date;

    constructor({ departments = [], managers = [], brands = [], seasons = [], contractor, dealId, discountFrom, discountTo, createDateFrom, createDateTo, loadDateFrom, loadDateTo }:
        {
            departments?: number[];
            managers?: number[];
            brands?: number[];
            seasons?: number[];
            contractor?: string;
            dealId?: number;
            discountFrom?: number;
            discountTo?: number;
            createDateFrom?: Date;
            createDateTo?: Date;
            loadDateFrom?: Date;
            loadDateTo?: Date;
        } = {}) {

        this.departments = departments;
        this.managers = managers;
        this.brands = brands;
        this.seasons = seasons;
        this.contractor = contractor;
        this.dealId = dealId;
        this.discountFrom = discountFrom;
        this.discountTo = discountTo;
        this.createDateFrom = createDateFrom;
        this.createDateTo = createDateTo;
        this.loadDateFrom = loadDateFrom;
        this.loadDateTo = loadDateTo;
    }
}

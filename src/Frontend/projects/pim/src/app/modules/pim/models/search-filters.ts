export class SearchFilters {
    public searchString: string[] = [];
    public selectedCategories: number[] = [];
    public withoutCategory: boolean;

    constructor(searchString: string[], selectedCategories: number[], withoutCategory: boolean) {
        this.searchString = searchString;
        this.selectedCategories = selectedCategories;
        this.withoutCategory = withoutCategory;
    }
}

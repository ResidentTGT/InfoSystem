import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { BackendApiService, DialogService, FileType, Module, PimProduct as Product, PimResourcePermissionsNames, ResourceAccessMethods, companyFile, UserService } from '@core';
import { combineLatest, EMPTY } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { Observable, Subscription } from 'rxjs/Rx';

@Component({
    selector: 'company-documents',
    templateUrl: './documents.component.html',
    styleUrls: ['./documents.component.scss'],
})
export class DocumentsComponent implements OnInit, OnDestroy {

    @Input()
    set setProduct(product: Product) {
        if (typeof product !== 'undefined') {
            this.product = product;
            this._getImages();

            this._getDocuments();
        }
    }

    @Output()
    public documentsChanged: EventEmitter<Product> = new EventEmitter<Product>();

    private _subscriptions: Subscription[] = [];

    public product: Product;
    public FileType = FileType;

    public mainImage: companyFile;
    public mediaContent: companyFile[] = [];
    public imagesLoading: boolean;

    public documents: companyFile[] = [];
    public documentsLoading: boolean;
    public displayedColumns = [
        'name', 'date', 'download', 'delete',
    ];

    public PimResourcePermissionsNames = PimResourcePermissionsNames;

    constructor(private _api: BackendApiService, private _dialogService: DialogService, public userService: UserService) { }

    ngOnInit() {
    }

    ngOnDestroy() {
        this._subscriptions.forEach((s) => s.unsubscribe());
    }

    public addMainImage(event) {
        if (event.type === 'change' && event.target.files && event.target.files[0]) {
            this.imagesLoading = true;
            const reader = new FileReader();
            const mainImage = new companyFile();

            mainImage.name = event.target.files[0].name;
            reader.readAsDataURL(event.target.files[0]);

            reader.onload = (e: any) => {
                mainImage.src = e.target.result;
                this._api.FileStorage.saveFile(event.target.files[0])
                    .pipe(
                        catchError((resp) => {
                            this.imagesLoading = false;
                            return this._dialogService
                                .openErrorDialog(`Не удалось сохранить установить главное изображение ${mainImage.name}.`, resp)
                                .afterClosed().pipe(
                                    switchMap((_) => EMPTY));
                        }),
                    )
                    .subscribe((file) => {
                        this.mainImage = mainImage;
                        this.mainImage.id = file.id;
                        this.mainImage.src = file.src;
                        this.mainImage.type = event.target.files[0].type.includes('image') ? FileType.Image : FileType.Video;
                        this.mediaContent.push(Object.assign(new companyFile(), this.mainImage));
                        this.imagesLoading = false;
                        this._emitDocumentsChangedEvent();
                    });
            };
        }
    }

    public addMedia(event, addInput) {
        if (event.type === 'change' && event.target.files && !!event.target.files.length) {
            const observables = [];
            this.imagesLoading = true;
            let count = 0;
            for (const file of event.target.files) {
                const reader = new FileReader();
                const media = new companyFile();

                media.name = file.name;
                reader.readAsDataURL(file);

                reader.onload = (e: any) => {
                    media.src = e.target.result;
                    observables.push(this._api.FileStorage.saveFile(file)
                        .pipe(
                            tap((fileEntity) => {
                                media.id = fileEntity.id;
                                media.src = fileEntity.src;
                                media.type = file.type.includes('image') ? FileType.Image : FileType.Video;
                                this.mediaContent.push(media);
                                this._emitDocumentsChangedEvent();
                            }),
                            catchError((resp) => {
                                this.imagesLoading = false;
                                return this._dialogService
                                    .openErrorDialog(`Не удалось сохранить дополнительное медиа ${media.name}.`, resp)
                                    .afterClosed().pipe(
                                        switchMap((_) => EMPTY));
                            }),
                        ));
                    count++;
                    if (count === event.target.files.length) {
                        combineLatest(observables)
                            .subscribe((_) => {
                                addInput.value = null;
                                this.imagesLoading = false;
                            });
                    }
                };
            }

        }
    }

    public clearMainImage() {
        this.mainImage = null;
        this._emitDocumentsChangedEvent();
    }

    public deleteMedia(image: companyFile) {
        if (this.mainImage && this.mainImage.id === image.id) {
            this.mainImage = null;
        }
        this.mediaContent.splice(this.mediaContent.indexOf(image), 1);
        this._emitDocumentsChangedEvent();
    }

    public makeMainImage(image: companyFile) {
        this.mainImage = Object.assign(new companyFile(), image);
        this._emitDocumentsChangedEvent();
    }

    public deleteDocument(file) {
        this.documents.splice(this.documents.indexOf(file), 1);
        this.documents = this.documents.slice();
        this._emitDocumentsChangedEvent();
    }

    public addDocument(event, docInput) {
        if (event.type === 'change' && event.target.files && !!event.target.files.length) {
            const observables = [];
            this.documentsLoading = true;
            let count = 0;
            for (const file of event.target.files) {
                const reader = new FileReader();
                const companyFile = new companyFile();

                companyFile.name = file.name;
                reader.readAsDataURL(file);

                reader.onload = (e: any) => {
                    companyFile.src = e.target.result;
                    observables.push(this._api.FileStorage.saveFile(file)
                        .pipe(
                            tap((newcompanyFile) => {
                                this.documents.push(newcompanyFile);
                                this.documents = this.documents.slice();
                                this._emitDocumentsChangedEvent();
                            }),
                            catchError((resp) => {
                                this.documentsLoading = false;
                                return this._dialogService
                                    .openErrorDialog(`Не удалось сохранить документ ${companyFile.name}.`, resp)
                                    .afterClosed().pipe(
                                        switchMap((_) => EMPTY));
                            }),
                        ),
                    );
                    count++;
                    if (count === event.target.files.length) {
                        combineLatest(observables)
                            .subscribe((_) => {
                                docInput.value = null;
                                this.documentsLoading = false;
                            });
                    }
                };
            }

        }
    }

    public openImage(image: companyFile): void {
        this._dialogService.openImageDialog(image.src);
    }

    private _emitDocumentsChangedEvent() {
        this.product.mainImgId = this.mainImage ? this.mainImage.id : null;
        this.product.imgsIds = this.mediaContent.filter((m) => m.type === FileType.Image).map((i) => i.id);
        this.product.videosIds = this.mediaContent.filter((m) => m.type === FileType.Video).map((i) => i.id);
        this.product.docsIds = this.documents.map((d) => d.id);
        this.documentsChanged.emit(this.product);
    }

    private _getImages() {
        this.mainImage = null;
        this.mediaContent = [];
        if (this.product.mainImgId) {
            this._getMainImage();
        }

        this._getMedia();
    }

    private _getDocuments() {
        this.documents = [];
        if (this.product.docsIds.length) {
            this.documentsLoading = true;
            this._subscriptions.push(
                this._api.FileStorage.getFiles(this.product.docsIds)
                    .pipe(
                        catchError((resp) => {
                            this.documentsLoading = false;
                            return this._dialogService
                                .openErrorDialog(`Не удалось загрузить документы.`, resp)
                                .afterClosed().pipe(
                                    switchMap((_) => EMPTY));
                        }),
                    )
                    .subscribe((docs: companyFile[]) => {
                        docs.forEach((d) => d.type = FileType.Document);
                        this.documents = docs;
                        this.documentsLoading = false;
                    }));
        }
    }

    private _getMainImage() {
        this.imagesLoading = true;
        this._subscriptions.push(
            this._api.FileStorage.getFiles([this.product.mainImgId])
                .pipe(
                    catchError((resp) => {
                        this.imagesLoading = false;
                        return this._dialogService
                            .openErrorDialog(`Не удалось загрузить главное изображение.`, resp)
                            .afterClosed().pipe(
                                switchMap((_) => EMPTY));
                    }),
                )
                .subscribe((images) => {
                    this.mainImage = images[0];
                    this.mainImage.type = FileType.Image;
                    this.imagesLoading = false;
                }));
    }
    private _getMedia() {
        this.imagesLoading = true;
        const observables = [];

        !!this.product.imgsIds.length ? observables.push(this._api.FileStorage.getFiles(this.product.imgsIds)) : observables.push(Observable.of([]));
        !!this.product.videosIds.length ? observables.push(this._api.FileStorage.getFiles(this.product.videosIds)) : observables.push(Observable.of([]));
        this._subscriptions.push(
            combineLatest(observables)
                .pipe(
                    catchError((resp) => {
                        this.imagesLoading = false;
                        return this._dialogService
                            .openErrorDialog(`Не удалось загрузить медиа-контент.`, resp)
                            .afterClosed().pipe(
                                switchMap((_) => EMPTY));
                    }),
                )
                .subscribe(([images, videos]: [any[], any[]]) => {
                    images.forEach((image) => image.type = FileType.Image);
                    videos.forEach((video) => video.type = FileType.Video);
                    this.mediaContent.push(...images);
                    this.mediaContent.push(...videos);
                    this.mediaContent.sort((a, b) => a.id - b.id);
                    this.imagesLoading = false;
                }));

    }

    public isPimResourceEditable(name: string): boolean {
        const modulPerm = this.userService.user.modulePermissions.filter((p) => p.module === Module.PIM)[0];
        return modulPerm && modulPerm.resourcePermissions.some((p) => p.name === name && !!(p.value & ResourceAccessMethods.Modify));
    }

    public isPimResourceDeletable(name: string): boolean {
        const modulPerm = this.userService.user.modulePermissions.filter((p) => p.module === Module.PIM)[0];
        return modulPerm && modulPerm.resourcePermissions.some((p) => p.name === name && !!(p.value & ResourceAccessMethods.Delete));
    }

}
